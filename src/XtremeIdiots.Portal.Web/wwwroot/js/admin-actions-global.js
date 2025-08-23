// Admin Actions (Global) page script extracted from inline Razor view
$(document).ready(function () {
    const table = $('#dataTable').DataTable({
        processing: true,
        serverSide: true,
        searching: false,
        responsive: true,
        responsive: { details: { type: 'inline', target: 'tr' } },
        autoWidth: true,
        order: [[0, 'desc']],
        columnDefs: [
            { targets: 0, width: '14%', responsivePriority: 1 }, // Created
            { targets: 1, width: '10%', responsivePriority: 5 }, // Game (icon)
            { targets: 2, width: '14%', responsivePriority: 2 }, // Type
            { targets: 3, width: '32%', responsivePriority: 3 }, // Player
            { targets: 4, width: '15%', responsivePriority: 6 }, // Admin
            { targets: 5, width: '15%', responsivePriority: 4 }  // Expires
        ],
        ajax: {
            url: '/AdminActions/GetAdminActionsAjax',
            dataSrc: 'data',
            contentType: 'application/json',
            type: 'POST',
            data: function (d) { return JSON.stringify(d); },
            beforeSend: function (xhr) {
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                xhr.setRequestHeader('RequestVerificationToken', token);
                const gt = $('#filterGameType').val();
                const adminActionFilter = $('#filterAdminActionFilter').val();
                const adminId = $('#filterAdminUserId').val();
                const baseUrl = '/AdminActions/GetAdminActionsAjax';
                let qs = [];
                if (gt) qs.push('gameType=' + encodeURIComponent(gt));
                if (adminActionFilter) qs.push('adminActionFilter=' + encodeURIComponent(adminActionFilter));
                if (adminId) qs.push('adminId=' + encodeURIComponent(adminId));
                this.url = baseUrl + (qs.length ? ('?' + qs.join('&')) : '');
            }
        },
        columns: [
            { data: 'created', name: 'created', sortable: true, render: function (data) { return '<span title="' + data + '">' + timeAgo(data) + '</span>'; } },
            { data: 'gameType', name: 'gameType', sortable: false, render: function (data) { return gameTypeIcon(data); } },
            { data: 'type', name: 'type', sortable: false, render: function (data) { return adminActionTypeIcon(data); } },
            { data: 'player', name: 'player', sortable: false, render: function (data, type, row) { return '<a href="/Players/Details/' + row.playerId + '">' + data + '</a><br/><small class="text-muted">' + (row.guid || '') + '</small>'; } },
            { data: 'admin', name: 'admin', sortable: false },
            { data: 'expires', name: 'expires', sortable: false, className: 'expires-cell', render: function (data) { return formatExpiryDate(data); } }
        ]
    });

    table.on('xhr.dt', function () { table.columns.adjust(); });
    $('#dataTable').on('init.dt', function () { setTimeout(function () { table.columns.adjust().draw(false); }, 350); });

    function applyGameColumnVisibility() {
        const hasSpecificGame = $('#filterGameType').val() !== '';
        table.column(1).visible(!hasSpecificGame, false);
    }

    applyGameColumnVisibility();

    $('#filterGameType,#filterAdminActionFilter').on('change', function () {
        applyGameColumnVisibility();
        table.ajax.reload(null, false);
    });

    $('#resetFilters').on('click', function () {
        const changed = $('#filterGameType').val() !== '' || $('#filterAdminActionFilter').val() !== '' || $('#filterAdminUserId').val() !== '';
        $('#filterGameType').val('');
        $('#filterAdminActionFilter').val('');
        $('#filterAdminUser').val('');
        $('#filterAdminUserId').val('');
        applyGameColumnVisibility();
        if (changed) {
            table.page('first').draw('page');
        } else {
            table.ajax.reload(null, false);
        }
    });

    // Initialize reusable user search autocomplete
    if (window.initUserSearchAutocomplete) {
        window.initUserSearchAutocomplete({
            inputSelector: '#filterAdminUser',
            hiddenSelector: '#filterAdminUserId',
            searchUrl: '/UserSearch/Users',
            minLength: 2,
            onSelect: function () { table.page('first').draw('page'); }
        });
    }
});
