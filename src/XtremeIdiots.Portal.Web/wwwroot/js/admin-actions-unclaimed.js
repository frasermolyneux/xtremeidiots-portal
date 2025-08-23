// Unclaimed Bans page script
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
            { targets: 0, width: '16%', responsivePriority: 1 }, // Created
            { targets: 1, width: '10%', responsivePriority: 5 }, // Game
            { targets: 2, width: '12%', responsivePriority: 2 }, // Type
            { targets: 3, width: '32%', responsivePriority: 3 }, // Player
            { targets: 4, width: '15%', responsivePriority: 4 }, // Expires
            { targets: 5, width: '15%', orderable: false, responsivePriority: 1 } // Action
        ],
        ajax: {
            url: '/AdminActions/GetUnclaimedAdminActionsAjax',
            dataSrc: 'data',
            contentType: 'application/json',
            type: 'POST',
            data: function (d) { return JSON.stringify(d); },
            beforeSend: function (xhr) {
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                xhr.setRequestHeader('RequestVerificationToken', token);
                const gt = $('#filterGameType').val();
                const baseUrl = '/AdminActions/GetUnclaimedAdminActionsAjax';
                let qs = [];
                if (gt) qs.push('gameType=' + encodeURIComponent(gt));
                this.url = baseUrl + (qs.length ? ('?' + qs.join('&')) : '');
            }
        },
        columns: [
            { data: 'created', name: 'created', sortable: true, render: function (data) { return '<span title="' + data + '">' + timeAgo(data) + '</span>'; } },
            { data: 'gameType', name: 'gameType', sortable: false, render: function (data) { return gameTypeIcon(data); } },
            { data: 'type', name: 'type', sortable: false, render: function (data) { return adminActionTypeIcon(data); } },
            { data: 'player', name: 'player', sortable: false, render: function (data, type, row) { return '<a href="/Players/Details/' + row.playerId + '">' + data + '</a><br/><small class="text-muted">' + (row.guid || '') + '</small>'; } },
            { data: 'expires', name: 'expires', sortable: false, className: 'expires-cell', render: function (data) { return formatExpiryDate(data); } },
            {
                data: null, name: 'action', sortable: false, className: 'text-center', render: function (data, type, row) {
                    if (!row.canClaim) return '';
                    return '<a href="/AdminActions/Claim/' + row.adminActionId + '" class="btn btn-outline-info btn-sm claim-btn"><i class="fa fa-user me-1"></i> Claim Ban</a>';
                }
            }
        ]
    });

    table.on('xhr.dt', function () { table.columns.adjust(); });
    $('#dataTable').on('init.dt', function () { setTimeout(function () { table.columns.adjust().draw(false); }, 350); });

    function applyGameColumnVisibility() {
        const hasSpecificGame = $('#filterGameType').val() !== '';
        table.column(1).visible(!hasSpecificGame, false);
    }
    applyGameColumnVisibility();

    $('#filterGameType').on('change', function () {
        applyGameColumnVisibility();
        table.ajax.reload(null, false);
    });

    $('#resetFilters').on('click', function () {
        const changed = $('#filterGameType').val() !== '';
        $('#filterGameType').val('');
        applyGameColumnVisibility();
        if (changed) {
            table.page('first').draw('page');
        } else {
            table.ajax.reload(null, false);
        }
    });
});
