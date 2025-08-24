// Players Index page script (refactored: removed separate Game column, added IP column)
$(document).ready(function () {
    const tableEl = $('#dataTable');
    const dataUrl = tableEl.data('source');
    const playersFilterSel = document.getElementById('filterPlayersFilter');

    const table = tableEl.DataTable({
        processing: true,
        serverSide: true,
        searchDelay: 800,
        stateSave: true,
        responsive: { details: { type: 'inline', target: 'tr' } },
        autoWidth: false,
        order: [[4, 'desc']], // Last Seen desc (index 4 after Name, IP, Guid, FirstSeen, LastSeen)
        stateSaveParams: function (settings, data) {
            data._playersStructureVersion = 3; // bump when changing column/filter persistence structure
            if (playersFilterSel) data.playersFilter = playersFilterSel.value || 'UsernameAndGuid';
            const gtSel = document.getElementById('filterGameType');
            if (gtSel) data.gameType = gtSel.value || '';
        },
        stateLoadParams: function (settings, data) {
            if (data._playersStructureVersion !== 3) return false; // invalidate older structures
            if (playersFilterSel && data.playersFilter) playersFilterSel.value = data.playersFilter;
            const gtSel = document.getElementById('filterGameType');
            if (gtSel && typeof data.gameType !== 'undefined') gtSel.value = data.gameType;
        },
        columnDefs: [
            { targets: 0, responsivePriority: 1, visible: true }, // Name (force visible)
            { targets: 1, responsivePriority: 5 }, // Player IP
            { targets: 2, responsivePriority: 3 }, // Guid
            { targets: 3, responsivePriority: 4 }, // First Seen
            { targets: 4, responsivePriority: 2 }  // Last Seen
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
                const gt = document.getElementById('filterGameType')?.value;
                const pf = playersFilterSel?.value || 'UsernameAndGuid';
                let base = dataUrl;
                // Build up URL: /Players/GetPlayersAjax[/GameType]?filter=PlayersFilterValue
                if (gt) base = dataUrl.replace(/\/Players\/GetPlayersAjax.*/, '/Players/GetPlayersAjax/' + encodeURIComponent(gt));
                const urlObj = new URL(base, window.location.origin);
                urlObj.searchParams.set('playersFilter', pf);
                this.url = urlObj.pathname + urlObj.search;
            }
        },
        columns: [
            { data: 'username', name: 'username', sortable: true, render: function (data, type, row) { return renderPlayerName(row['gameType'], row['username'], row['playerId']); } },
            {
                data: 'ipAddress', name: 'ipAddress', sortable: false, render: function (data, type, row) {
                    return data ? formatIPAddress(data, row['proxyCheckRiskScore'], row['isProxy'], row['isVpn'], row['proxyType'], row['countryCode'], true) : '';
                }
            },
            { data: 'guid', name: 'guid', sortable: false, defaultContent: '' },
            { data: 'firstSeen', name: 'firstSeen', sortable: true, render: function (data) { return data ? ('<span title="' + data + '">' + formatDateTime(data, { showRelative: true }) + '</span>') : ''; } },
            { data: 'lastSeen', name: 'lastSeen', sortable: true, render: function (data) { return data ? ('<span title="' + data + '">' + timeAgo(data) + '</span>') : ''; } }
        ]
    });

    function relocateSearch() {
        try {
            const filters = document.getElementById('playersFilters');
            const dtFilter = document.getElementById('dataTable_filter');
            if (!filters || !dtFilter) return;
            if (dtFilter.classList) dtFilter.classList.add('filter-group');
            const label = dtFilter.querySelector('label');
            if (label) {
                const input = label.querySelector('input');
                if (input) {
                    if (input.classList) input.classList.add('form-control');
                    input.placeholder = 'Search players...';
                    label.textContent = '';
                    const newLabel = document.createElement('label');
                    newLabel.className = 'form-label';
                    newLabel.setAttribute('for', input.id || 'globalPlayersSearch');
                    if (!input.id) input.id = 'globalPlayersSearch';
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
        } catch { /* swallow */ }
    }

    table.on('init.dt', function(){ relocateSearch(); });
    setTimeout(relocateSearch, 1000);

    function reloadTable() { table.ajax.reload(null, false); }

    document.getElementById('filterGameType')?.addEventListener('change', reloadTable);
    playersFilterSel?.addEventListener('change', reloadTable);

    document.getElementById('resetFilters')?.addEventListener('click', function () {
        const sel = document.getElementById('filterGameType');
        let changed = false;
        if (sel && sel.value !== '') { sel.value = ''; changed = true; }
        if (playersFilterSel && playersFilterSel.value !== 'UsernameAndGuid') { playersFilterSel.value = 'UsernameAndGuid'; changed = true; }
        if (table.search()) { table.search(''); changed = true; }
        if (changed) table.page('first');
        table.draw(false);
    });

    const iboxContent = tableEl.closest('.ibox-content')[0];
    if (iboxContent && iboxContent.classList) iboxContent.classList.add('datatable-tight');
});
