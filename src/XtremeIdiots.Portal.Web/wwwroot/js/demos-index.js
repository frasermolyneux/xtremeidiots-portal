// Demos Index page script (mirrors players-index.js patterns)
$(document).ready(function () {
    const tableEl = $('#dataTable');
    const dataUrlBase = tableEl.data('source');
    const gtSel = document.getElementById('filterGameType');

    // Human readable byte size formatting (KiB, MiB, GiB) - small utility scoped to this page
    function formatBytes(bytes) {
        if (bytes === null || bytes === undefined || isNaN(bytes)) return '';
        const thresh = 1024;
        if (Math.abs(bytes) < thresh) return bytes + ' B';
        const units = ['KiB', 'MiB', 'GiB', 'TiB', 'PiB'];
        let u = -1;
        let value = bytes;
        do { value /= thresh; ++u; } while (Math.abs(value) >= thresh && u < units.length - 1);
        return value.toFixed(value >= 100 ? 0 : value >= 10 ? 1 : 2) + ' ' + units[u];
    }

    function formatBytes(bytes) {
        if (bytes === null || bytes === undefined) return '';
        const num = Number(bytes);
        if (!Number.isFinite(num)) return '';
        if (num < 1024) return num + ' B';
        const units = ['KB', 'MB', 'GB', 'TB'];
        let value = num;
        let i = 0;
        while (value >= 1024 && i < units.length - 1) {
            value /= 1024;
            i++;
        }
        // For large or small values adjust precision
        const precision = value >= 100 || value < 10 ? 1 : 2;
        return value.toFixed(precision) + ' ' + units[i];
    }

    const table = tableEl.DataTable({
        processing: true,
        serverSide: true,
        searchDelay: 800,
        stateSave: true,
        responsive: { details: { type: 'inline', target: 'tr' } },
        autoWidth: false,
        order: [[2, 'desc']], // Date desc
        stateSaveParams: function (settings, data) {
            data._demosStructureVersion = 1; // bump when altering filters/columns persistence
            if (gtSel) data.gameType = gtSel.value || '';
        },
        stateLoadParams: function (settings, data) {
            if (data._demosStructureVersion !== 1) return false; // invalidate older persisted state
            if (gtSel && typeof data.gameType !== 'undefined') gtSel.value = data.gameType;
        },
        columnDefs: [
            { targets: 0, responsivePriority: 1 }, // Game icon
            { targets: 1, responsivePriority: 2 }, // Name
            { targets: 2, responsivePriority: 3 }, // Date
            { targets: 3, responsivePriority: 6 }, // Map
            { targets: 4, responsivePriority: 7 }, // Mod
            { targets: 5, responsivePriority: 8 }, // GameType (raw)
            { targets: 6, responsivePriority: 5 }, // Server
            { targets: 7, responsivePriority: 4 }, // Size
            { targets: 8, responsivePriority: 9 }, // Uploaded By
            { targets: 9, responsivePriority: 10 } // Actions
        ],
        ajax: {
            url: dataUrlBase,
            dataSrc: 'data',
            contentType: 'application/json',
            type: 'POST',
            data: function (d) { return JSON.stringify(d); },
            beforeSend: function (xhr) {
                const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenInput) xhr.setRequestHeader('RequestVerificationToken', tokenInput.value);
                let url = dataUrlBase;
                const gt = gtSel?.value;
                if (gt) {
                    // Replace /Demos/GetDemoListAjax.* with game-specific endpoint
                    url = dataUrlBase.replace(/\/Demos\/GetDemoListAjax.*/, '/Demos/GetDemoListAjax/' + encodeURIComponent(gt));
                }
                this.url = url;
            }
        },
        columns: [
            { data: 'game', name: 'game', sortable: true, render: function (data, type, row) { return gameTypeIcon(row['game']); } },
            { data: 'name', name: 'name', sortable: true, render: function (data, type, row) { return downloadDemoLink(row['name'], row['demoId']); } },
            { data: 'date', name: 'date', sortable: true, render: function (data) { return data ? '<span title="' + data + '">' + formatDateTime(data, { showRelative: true }) + '</span>' : ''; } },
            { data: 'map', name: 'map', sortable: false },
            { data: 'mod', name: 'mod', sortable: false },
            { data: 'gameType', name: 'gameType', sortable: false },
            { data: 'server', name: 'server', sortable: false },
            { data: 'size', name: 'size', sortable: false, render: function (data) { return formatBytes(data); } },
            { data: 'uploadedBy', name: 'uploadedBy', sortable: true },
            { data: null, defaultContent: '', orderable: false, render: function (data, type, row) { return row['showDeleteLink'] === true ? deleteDemoLink(row['demoId'], gtSel?.value) : ''; } }
        ]
    });

    function relocateSearch() {
        try {
            const filters = document.getElementById('demosFilters');
            const dtFilter = document.getElementById('dataTable_filter');
            if (!filters || !dtFilter) return;
            if (dtFilter.classList) dtFilter.classList.add('filter-group');
            const label = dtFilter.querySelector('label');
            if (label) {
                const input = label.querySelector('input');
                if (input) {
                    if (input.classList) input.classList.add('form-control');
                    input.placeholder = 'Search demos...';
                    label.textContent = '';
                    const newLabel = document.createElement('label');
                    newLabel.className = 'form-label';
                    newLabel.setAttribute('for', input.id || 'globalDemosSearch');
                    if (!input.id) input.id = 'globalDemosSearch';
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
        } catch { /* ignore */ }
    }

    table.on('init.dt', function () { relocateSearch(); });
    setTimeout(relocateSearch, 800);

    function reload() { table.ajax.reload(null, false); }
    gtSel?.addEventListener('change', reload);

    document.getElementById('resetFilters')?.addEventListener('click', function () {
        let changed = false;
        if (gtSel && gtSel.value !== '') { gtSel.value = ''; changed = true; }
        if (table.search()) { table.search(''); changed = true; }
        if (changed) table.page('first');
        table.draw(false);
    });

    const iboxContent = tableEl.closest('.ibox-content')[0];
    if (iboxContent && iboxContent.classList) iboxContent.classList.add('datatable-tight');
});
