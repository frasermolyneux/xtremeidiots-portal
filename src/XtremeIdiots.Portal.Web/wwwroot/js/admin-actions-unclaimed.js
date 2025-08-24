// Unclaimed Bans page script
$(document).ready(function () {
    const table = $('#dataTable').DataTable({
        processing: true,
        serverSide: true,
        searching: false,
        responsive: { details: { type: 'inline', target: 'tr' } },
        autoWidth: false,
        order: [[0, 'desc']],
        columnDefs: [
            // Priority (keep lower numbers visible longest)
            { targets: 1, responsivePriority: 1 }, // Game
            { targets: 2, responsivePriority: 2 }, // Type
            { targets: 3, responsivePriority: 3 }, // Player
            { targets: 5, orderable: false, responsivePriority: 4 }, // Action
            { targets: 4, responsivePriority: 5 }, // Expires
            { targets: 0, responsivePriority: 6 } // Created
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
    $('#dataTable').on('init.dt', function () { setTimeout(function () { table.columns.adjust().draw(false); }, 250); });

    function applyGameFilterVisibility() {
        const hasSpecificGame = $('#filterGameType').val() !== '';
        // Only hide Game column when a specific game is selected; Created handled by responsive priorities only
        table.column(1).visible(!hasSpecificGame, false);
    }
    applyGameFilterVisibility();

    $('#filterGameType').on('change', function () {
        applyGameFilterVisibility();
        table.ajax.reload(null, false);
    });

    $('#resetFilters').on('click', function () {
        const changed = $('#filterGameType').val() !== '';
        $('#filterGameType').val('');
        applyGameFilterVisibility();
        if (changed) {
            table.page('first').draw('page');
        } else {
            table.ajax.reload(null, false);
        }
    });
    // No manual resize handler – rely fully on DataTables Responsive
});
