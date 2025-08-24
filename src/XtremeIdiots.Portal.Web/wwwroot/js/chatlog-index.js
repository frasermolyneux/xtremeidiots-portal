// Chat Log Index page script - mirrors Players Index patterns (responsive, external filters, state)
$(document).ready(function () {
    const tableEl = $('#dataTable');
    const dataUrl = tableEl.data('source');
    const showServer = tableEl.data('show-server') === true || tableEl.data('show-server') === 'True' || tableEl.data('show-server') === 'true';
    const lockedSel = document.getElementById('filterLocked');
    const gameTypeSel = document.getElementById('filterGameType');

    const structureVersion = 4; // bumped after switching locked filter to select

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
                        : row['gameType'];

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
                if (row['locked']) {
                    return '<i class="fa fa-lock text-warning" title="Locked"></i>';
                }
                return '<i class="fa fa-unlock text-muted" title="Unlocked"></i>';
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
        },
        stateLoadParams: function (settings, data) {
            if (data._chatlogStructureVersion !== structureVersion) return false;
            if (lockedSel && typeof data.lockedOnly !== 'undefined') lockedSel.value = data.lockedOnly ? 'true' : '';
            if (gameTypeSel && typeof data.gameType !== 'undefined') gameTypeSel.value = data.gameType;
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
                if (gameTypeSel && gameTypeSel.value) {
                    base = dataUrl.replace(/\/ServerAdmin\/GetChatLogAjax.*/, '/ServerAdmin/GetGameChatLogAjax/' + encodeURIComponent(gameTypeSel.value));
                }
                // Append lockedOnly flag (true only when checked)
                const urlObj = new URL(base, window.location.origin);
                if (lockedSel && lockedSel.value === 'true') urlObj.searchParams.set('lockedOnly', 'true'); else urlObj.searchParams.delete('lockedOnly');
                this.url = urlObj.pathname + urlObj.search;
            }
        },
        columns: columns
    });

    // Integrate search box into filters bar
    (function relocateSearch() {
        const filters = document.getElementById('chatLogFilters');
        const dtFilter = document.getElementById('dataTable_filter');
        if (!filters || !dtFilter) return;
        dtFilter.classList.add('filter-group');
        const label = dtFilter.querySelector('label');
        if (label) {
            const input = label.querySelector('input');
            if (input) {
                input.classList.add('form-control');
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
        // Place search filter before reset group (same pattern as players index)
        if (resetGroup && resetGroup.parentElement === filters) {
            filters.insertBefore(dtFilter, resetGroup);
        } else {
            filters.appendChild(dtFilter);
        }
    })();

    lockedSel?.addEventListener('change', function () { table.ajax.reload(null, false); });
    gameTypeSel?.addEventListener('change', function () { table.ajax.reload(null, false); });

    document.getElementById('resetFilters')?.addEventListener('click', function () {
        let changed = false;
        if (lockedSel && lockedSel.value !== '') { lockedSel.value = ''; changed = true; }
        if (gameTypeSel && gameTypeSel.value !== '') { gameTypeSel.value = ''; changed = true; }
        if (table.search()) { table.search(''); changed = true; }
        if (changed) table.page('first');
        table.ajax.reload(null, false);
    });

    const iboxContent = tableEl.closest('.ibox-content');
    if (iboxContent) iboxContent.classList.add('datatable-tight');
});
