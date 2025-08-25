// Users index page script (extracted from inline Razor)
$(document).ready(function () {
    const $tableEl = $('#dataTable');
    const $search = $('#filterSearch');
    const $userFlag = $('#filterUserFlag');
    const $reset = $('#resetFilters');

    const antiForgeryToken = function () {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    };

    // Whitelisted claim types to show as roles
    const ROLE_CLAIM_TYPES = new Set([
        'SeniorAdmin', 'HeadAdmin', 'GameAdmin', 'Moderator', 'ServerAdmin', 'BanFileMonitor', 'RconCredentials', 'FtpCredentials'
    ]);

    function renderRoles(row) {
        const claims = row.userProfileClaims || [];
        if (!Array.isArray(claims) || claims.length === 0) return '';
        const types = [...new Set(claims
            .map(c => c.claimType)
            .filter(t => ROLE_CLAIM_TYPES.has(t)))];
        if (types.length === 0) return '';
        return types.map(t => '<span class="badge text-bg-secondary me-1 mb-1">' + t + '</span>').join('');
    }

    function forumProfileLink(id, displayName) {
        if (!id) return '';
        const name = (displayName || '').toString().trim().toLowerCase();
        if (!name) return id; // fallback no link if no name
        const slug = name
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/^-+|-+$/g, '')
            .substring(0, 60); // trim excessively long
        const url = 'https://www.xtremeidiots.com/profile/' + id + '-' + slug + '/';
        return '<a href="' + url + '" target="_blank" rel="noopener noreferrer" title="Open forum profile in new tab">' + id + '</a>';
    }

    const table = $tableEl.DataTable({
        processing: true,
        serverSide: true,
        searching: false, // we'll handle via external box
        stateSave: true,
        searchDelay: 600,
        order: [[2, 'asc']], // displayName column
        responsive: { details: { type: 'inline', target: 'tr' } },
        autoWidth: false,
        columnDefs: [
            { targets: 2, responsivePriority: 1 }, // Username
            { targets: 3, responsivePriority: 2 }, // Email
            { targets: 4, responsivePriority: 5 }, // Roles
            { targets: 5, responsivePriority: 3 }  // Logout
        ],
        ajax: {
            url: '/User/GetUsersAjax',
            dataSrc: 'data',
            contentType: 'application/json',
            type: 'POST',
            data: function (d) { return JSON.stringify(d); },
            beforeSend: function (xhr) {
                const token = antiForgeryToken();
                if (token) xhr.setRequestHeader('RequestVerificationToken', token);
                const baseUrl = '/User/GetUsersAjax';
                const flagVal = $userFlag.val();
                this.url = flagVal ? (baseUrl + '?userFlag=' + encodeURIComponent(flagVal)) : baseUrl;
            }
        },
        columns: [
            { data: 'xtremeIdiotsForumId', name: 'xtremeIdiotsForumId', sortable: false, render: function (data, type, row) { return forumProfileLink(data, row.displayName); } },
            { data: 'userProfileId', name: 'userProfileId', sortable: false, render: function (data) { return '<a href="/User/ManageProfile/' + data + '">' + data + '</a>'; } },
            { data: 'displayName', name: 'displayName', sortable: true },
            { data: 'email', name: 'email', sortable: false },
            { data: null, name: 'roles', sortable: false, defaultContent: '', render: function (data, type, row) { return renderRoles(row); } },
            { data: null, defaultContent: '', sortable: false, render: function (data, type, row) { return logOutUserLink(row['xtremeIdiotsForumId'], '<input name="__RequestVerificationToken" type="hidden" value="' + antiForgeryToken() + '" />'); } }
        ]
    });

    // External search with debounce (reuses DataTables search API)
    let searchTimer = null;
    $search.on('input', function () {
        clearTimeout(searchTimer);
        const term = this.value.trim();
        searchTimer = setTimeout(function () { table.search(term).draw(); }, 500);
    });

    $userFlag.on('change', function () {
        table.page('first').draw('page');
    });

    $reset.on('click', function () {
        const hadValues = $search.val() || $userFlag.val();
        if ($search.val()) $search.val('');
        if ($userFlag.val()) $userFlag.val('');
        if (hadValues) {
            table.search('').page('first').draw('page');
        } else {
            table.ajax.reload(null, false);
        }
    });
});
