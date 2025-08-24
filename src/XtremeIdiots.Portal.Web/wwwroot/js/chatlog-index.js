// Chat Log Index page script - mirrors Players Index patterns (responsive, external filters, state)
$(document).ready(function () {
    const tableEl = $('#dataTable');
    const dataUrl = tableEl.data('source');
    const showServer = tableEl.data('show-server') === true || tableEl.data('show-server') === 'True' || tableEl.data('show-server') === 'true';
    const lockedSel = document.getElementById('filterLocked');
    const gameTypeSel = document.getElementById('filterGameType');
    // Determine fixed game type (when no selector rendered)
    let fixedGameType = '';
    if (!gameTypeSel) {
        try {
            var m = (dataUrl || '').match(/GetGameChatLogAjax\/(\w+)/);
            if (m) fixedGameType = m[1];
        } catch { /* ignore */ }
    }
    let serverSel = document.getElementById('filterGameServer');
    let allServers = []; // cached server list
    let selectedServerId = '';
    // (Optional) meta tag override
    if (!gameTypeSel && !fixedGameType) {
        const gtMeta = document.querySelector('meta[name="page-game-type"]');
        if (gtMeta) fixedGameType = gtMeta.getAttribute('content') || '';
    }

    const structureVersion = 5; // bumped after adding server filter

    const columns = [
        { data: 'timestamp', name: 'timestamp', sortable: true, render: function (data) { return data ? ('<span title="' + data + '">' + formatDateTime(data, { showRelative: true }) + '</span>') : ''; } },
        {
            data: 'username', name: 'username', sortable: false, render: function (data, type, row) {
                // Prefer numeric enum (player.gameType or gameServer.gameType); fall back to top-level gameType.
                // Some chat log payloads may include string enum names instead of numeric values.
                var gameTypeVal = (row['player'] && row['player']['gameType'] !== undefined)
                    ? row['player']['gameType']
                    : (row['gameServer'] && row['gameServer']['gameType'] !== undefined)
                        ? row['gameServer']['gameType']
                        : (row['gameType'] !== undefined ? row['gameType'] : fixedGameType);

                var safeUsername = escapeHtml(row['username'] || '');
                var playerId = row['playerId'];

                function usernameAnchor() {
                    return playerId ? ("<a href='/Players/Details/" + playerId + "'>" + safeUsername + "</a>") : safeUsername;
                }

                if (gameTypeVal === undefined || gameTypeVal === null) {
                    return safeUsername; // no icon if we can't determine game type
                }

                // If it's numeric (enum), use enum icon mapping; otherwise treat as string name.
                if (typeof gameTypeVal === 'number') {
                    return gameTypeIconEnum(gameTypeVal) + ' ' + usernameAnchor();
                }
                return gameTypeIcon(gameTypeVal) + ' ' + usernameAnchor();
            }
        },
        { data: 'chatType', name: 'chatType', sortable: false },
        { data: 'message', name: 'message', sortable: false, render: function (data) { return data ? escapeHtml(data) : ''; } }
    ];
    if (showServer) {
        // Use data: null so DataTables doesn't expect a primitive at serverName path; render from nested object
        columns.push({ data: null, name: 'serverName', sortable: false, render: function (data, type, row) { return row['gameServer']?.['liveTitle'] || row['serverName'] || ''; } });
    }
    columns.push(
        {
            data: 'locked',
            name: 'locked',
            sortable: false,
            render: function (data, type, row) {
                var id = row['chatMessageId'];
                if (!id) return '';
                if (row['locked']) {
                    return '<button type="button" class="btn btn-link p-0 lock-toggle" data-id="' + id + '" data-locked="true" title="Click to unlock"><i class="fa fa-lock text-warning"></i></button>';
                }
                return '<button type="button" class="btn btn-link p-0 lock-toggle" data-id="' + id + '" data-locked="false" title="Click to lock"><i class="fa fa-unlock text-muted"></i></button>';
            }
        },
        { data: 'chatMessageId', name: 'chatMessageId', sortable: false, render: function (data, type, row) { return chatLogUrl(row['chatMessageId']); } }
    );

    const table = tableEl.DataTable({
        processing: true,
        serverSide: true,
        searchDelay: 700,
        stateSave: true,
        responsive: { details: { type: 'inline', target: 'tr' } },
        autoWidth: false,
        order: [[0, 'desc']], // timestamp desc
        stateSaveParams: function (settings, data) {
            data._chatlogStructureVersion = structureVersion;
            if (lockedSel) data.lockedOnly = (lockedSel.value === 'true') ? 1 : 0;
            if (gameTypeSel) data.gameType = gameTypeSel.value || '';
            if (serverSel) data.serverId = selectedServerId || '';
        },
        stateLoadParams: function (settings, data) {
            if (data._chatlogStructureVersion !== structureVersion) return false;
            if (lockedSel && typeof data.lockedOnly !== 'undefined') lockedSel.value = data.lockedOnly ? 'true' : '';
            if (gameTypeSel) data.gameType = gameTypeSel.value || '';
            if (serverSel && typeof data.serverId !== 'undefined') selectedServerId = data.serverId;
        },
        columnDefs: [
            { targets: 0, responsivePriority: 1 }, // timestamp
            { targets: 1, responsivePriority: 2 }, // username
            { targets: 3, responsivePriority: 3 }, // message
            { targets: -1, responsivePriority: 4 } // link
        ],
        ajax: {
            url: dataUrl,
            dataSrc: 'data',
            contentType: 'application/json',
            type: 'POST',
            data: function (d) { return JSON.stringify(d); },
            beforeSend: function (xhr) {
                const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenInput) xhr.setRequestHeader('RequestVerificationToken', tokenInput.value);
                // Compute base endpoint depending on game type selection
                let base = dataUrl;
                if (selectedServerId) {
                    base = '/ServerAdmin/GetServerChatLogAjax/' + selectedServerId;
                } else if (gameTypeSel && gameTypeSel.value) {
                    base = dataUrl.replace(/\/ServerAdmin\/GetChatLogAjax.*/, '/ServerAdmin/GetGameChatLogAjax/' + encodeURIComponent(gameTypeSel.value));
                } else if (!gameTypeSel && fixedGameType) {
                    base = '/ServerAdmin/GetGameChatLogAjax/' + encodeURIComponent(fixedGameType);
                }
                // Append lockedOnly flag (true only when checked)
                const urlObj = new URL(base, window.location.origin);
                if (lockedSel && lockedSel.value === 'true') urlObj.searchParams.set('lockedOnly', 'true'); else urlObj.searchParams.delete('lockedOnly');
                this.url = urlObj.pathname + urlObj.search;
            }
        },
        columns: columns
    });

    function relocateSearch() {
        try {
            const filters = document.getElementById('chatLogFilters');
            const dtFilter = document.getElementById('dataTable_filter');
            if (!filters || !dtFilter) {
                console.warn('[ChatLog] relocateSearch: filter elements not ready');
                return;
            }
            if (dtFilter.classList) dtFilter.classList.add('filter-group');
            const label = dtFilter.querySelector('label');
            if (label) {
                const input = label.querySelector('input');
                if (input) {
                    if (input.classList) input.classList.add('form-control');
                    input.placeholder = 'Search chat...';
                    label.textContent = '';
                    const newLabel = document.createElement('label');
                    newLabel.className = 'form-label';
                    newLabel.setAttribute('for', input.id || 'globalChatSearch');
                    if (!input.id) input.id = 'globalChatSearch';
                    newLabel.textContent = 'Search';
                    dtFilter.appendChild(newLabel);
                    dtFilter.appendChild(input);
                }
            }
            const resetBtn = document.getElementById('resetFilters');
            const resetGroup = resetBtn ? resetBtn.closest('.filter-group') : null;
            if (resetGroup && resetGroup.parentElement === filters) {
                filters.insertBefore(dtFilter, resetGroup);
            } else {
                filters.appendChild(dtFilter);
            }
            console.log('[ChatLog] relocateSearch complete');
        } catch (e) {
            console.warn('[ChatLog] relocateSearch error', e);
        }
    }

    // Run after DataTables initialization to avoid timing issues
    table.on('init.dt', function () { relocateSearch(); });
    // Fallback in case init event missed (safety timeout)
    setTimeout(relocateSearch, 1200);

    if (lockedSel) lockedSel.addEventListener('change', function () { table.ajax.reload(null, false); });
    if (gameTypeSel) gameTypeSel.addEventListener('change', function () {
        filterServersByGame();
        table.ajax.reload(null, false);
    });
    if (serverSel) serverSel.addEventListener('change', function () {
        selectedServerId = serverSel.value;
        handleServerColumnVisibility();
        table.ajax.reload(null, false);
    });

    document.getElementById('resetFilters')?.addEventListener('click', function () {
        let changed = false;
        if (lockedSel && lockedSel.value !== '') { lockedSel.value = ''; changed = true; }
        if (gameTypeSel && gameTypeSel.value !== '') { gameTypeSel.value = ''; changed = true; }
        if (serverSel && serverSel.value !== '') { serverSel.value = ''; selectedServerId = ''; changed = true; handleServerColumnVisibility(); }
        if (table.search()) { table.search(''); changed = true; }
        if (changed) table.page('first');
        table.ajax.reload(null, false);
    });

    const iboxContent = tableEl && tableEl.length ? tableEl[0].closest('.ibox-content') : null;
    if (iboxContent && iboxContent.classList) {
        iboxContent.classList.add('datatable-tight');
    } else {
        console.warn('[ChatLog] ibox-content not found or lacks classList');
    }

    // ----- Server Filter Population & Behavior -----
    function populateServers() {
        if (!serverSel) return;
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const headers = {};
        if (tokenInput) headers['RequestVerificationToken'] = tokenInput.value;
        fetch('/ServerAdmin/GetChatLogServers', { credentials: 'same-origin', headers: headers })
            .then(r => {
                if (!r.ok) {
                    return [];
                }
                return r.json().catch(() => []);
            })
            .then(list => {
                if (!Array.isArray(list)) return;
                allServers = list;
                rebuildServerOptions();
                if (allServers.length === 0) {
                    var opt = document.createElement('option');
                    opt.value = '';
                    opt.textContent = 'No servers available';
                    serverSel.appendChild(opt);
                }
                // restore selection if came from state
                if (selectedServerId) {
                    serverSel.value = selectedServerId;
                    handleServerColumnVisibility();
                }
            })
            .catch(() => { /* swallow */ });
    }

    function rebuildServerOptions() {
        if (!serverSel) return;
        var current = serverSel.value;
        serverSel.innerHTML = '<option value="">All Servers</option>';
        var gtFilter = gameTypeSel && gameTypeSel.value ? gameTypeSel.value : fixedGameType;
        allServers
            .filter(s => !gtFilter || s.gameType == gtFilter || s.gameType === parseInt(gtFilter))
            .forEach(s => {
                var opt = document.createElement('option');
                opt.value = s.id;
                opt.textContent = s.title;
                opt.setAttribute('data-gametype', s.gameType);
                serverSel.appendChild(opt);
            });
        if (current && [...serverSel.options].some(o => o.value === current)) serverSel.value = current; else if (current) selectedServerId = '';
    }

    function filterServersByGame() {
        rebuildServerOptions();
    }

    function handleServerColumnVisibility() {
        if (!showServer) return; // original table suppressed server column
        var serverColumnIndex = columns.length - 2; // before locked + link? depends on showServer true
        if (showServer) {
            // Recompute index: timestamp(0), username(1), type(2), message(3), server(4), locked(5), link(6)
            serverColumnIndex = 4;
        }
        if (selectedServerId) {
            table.column(serverColumnIndex).visible(false);
        } else {
            table.column(serverColumnIndex).visible(true);
        }
    }

    function initServerDropdownIfReady(reason) {
        if (!serverSel) serverSel = document.getElementById('filterGameServer');
        if (!serverSel) return false;
        console.log('[ChatLog] Server select detected (' + reason + '), initiating server list fetch');
        populateServers('initial');
        setTimeout(function () { if (allServers.length === 0) { console.log('[ChatLog] Retrying server list fetch (initial list empty)'); populateServers('retry-1'); } }, 1500);
        setTimeout(function () { if (allServers.length === 0) { console.log('[ChatLog] Second retry server list fetch (still empty)'); populateServers('retry-2'); } }, 4000);
        return true;
    }

    // Simplified initialization: if select present populate immediately
    if (serverSel) populateServers();

    // Initial adjust if state loaded a server
    if (selectedServerId) handleServerColumnVisibility();

    // Inline lock/unlock handling
    document.addEventListener('click', function (e) {
        var btn = e.target.closest && e.target.closest('.lock-toggle');
        if (!btn) return;
        var id = btn.getAttribute('data-id');
        if (!id) return;
        // Optimistic: swap icon immediately
        var currentlyLocked = btn.getAttribute('data-locked') === 'true';
        var newLocked = !currentlyLocked;
        btn.setAttribute('data-locked', newLocked ? 'true' : 'false');
        btn.innerHTML = newLocked
            ? '<i class="fa fa-lock text-warning"></i>'
            : '<i class="fa fa-unlock text-muted"></i>';
        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        var headers = { 'X-Requested-With': 'XMLHttpRequest' };
        if (tokenInput) headers['RequestVerificationToken'] = tokenInput.value;
        fetch('/ServerAdmin/ToggleChatMessageLock/' + id, {
            method: 'POST',
            headers: headers,
            credentials: 'same-origin'
        }).then(function (r) { return r.ok ? r.json().catch(function () { return {}; }) : {}; })
            .then(function (resp) {
                if (!resp || resp.success !== true) {
                    // revert optimistic change if failed
                    btn.setAttribute('data-locked', currentlyLocked ? 'true' : 'false');
                    btn.innerHTML = currentlyLocked
                        ? '<i class="fa fa-lock text-warning"></i>'
                        : '<i class="fa fa-unlock text-muted"></i>';
                    if (typeof toastr !== 'undefined') toastr.error('Failed to toggle lock ' + (resp && resp.error ? '(' + resp.error + ')' : ''));
                } else {
                    if (typeof toastr !== 'undefined') toastr.success('Chat message ' + (resp.locked ? 'locked' : 'unlocked'));
                }
                table.ajax.reload(null, false);
            })
            .catch(function () {
                btn.setAttribute('data-locked', currentlyLocked ? 'true' : 'false');
                btn.innerHTML = currentlyLocked
                    ? '<i class="fa fa-lock text-warning"></i>'
                    : '<i class="fa fa-unlock text-muted"></i>';
                if (typeof toastr !== 'undefined') toastr.error('Failed to toggle lock');
            });
    });
});
