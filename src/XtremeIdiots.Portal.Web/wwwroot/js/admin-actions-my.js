(function () {
    const tableEl = document.getElementById('myActionsTable');
    if (!tableEl) return;

    const afToken = document.querySelector('#__af input[name="__RequestVerificationToken"]')?.value;

    function calcScrollY() {
        // Use parent ibox height to fit within grid cell
        const ibox = tableEl.closest('.ibox');
        if (ibox) {
            const title = ibox.querySelector('.ibox-title');
            const total = ibox.clientHeight;
            const titleH = title ? title.offsetHeight : 0;
            const headerH = tableEl.tHead ? tableEl.tHead.offsetHeight : 34;
            const reserve = 24; // padding / pagination
            return Math.max(200, total - titleH - headerH - reserve) + 'px';
        }
        const rect = tableEl.getBoundingClientRect();
        const vpH = window.innerHeight || document.documentElement.clientHeight;
        const bottomPadding = 40;
        return Math.max(200, vpH - rect.top - bottomPadding) + 'px';
    }

    const dt = $(tableEl).DataTable({
        processing: true,
        serverSide: true,
        searching: false,
        lengthChange: false,
        pageLength: 25,
        order: [[0, 'desc']],
        scrollY: calcScrollY(),
        scrollCollapse: true,
        ajax: {
            url: '/AdminActions/GetMyAdminActionsAjax',
            type: 'POST',
            contentType: 'application/json',
            beforeSend: function (xhr) { if (afToken) xhr.setRequestHeader('RequestVerificationToken', afToken); },
            data: function (d) { return JSON.stringify(d); },
            dataSrc: function (json) { return json.data; }
        },
        columns: [
            { data: 'created', render: function (d) { return window.timeAgo ? window.timeAgo(d) : d; } },
            { data: 'gameType', className: 'hide-sm', render: function (d) { return d ? (window.gameTypeIcon ? window.gameTypeIcon(d) : d) : ''; } },
            { data: 'type', render: function (d) { return window.adminActionTypeIcon ? window.adminActionTypeIcon(d) : d; } },
            { data: 'player' },
            { data: 'text', visible: false }
        ]
    });

    function reload() {
        const params = new URLSearchParams();
        const gameType = document.getElementById('filterMyGameType').value;
        const actionFilter = document.getElementById('filterMyAdminActionFilter').value;
        if (gameType) params.append('gameType', gameType);
        if (actionFilter) params.append('adminActionFilter', actionFilter);
        const base = '/AdminActions/GetMyAdminActionsAjax';
        dt.ajax.url(base + (params.toString() ? ('?' + params.toString()) : ''));
        dt.ajax.reload();
    }

    document.getElementById('filterMyGameType').addEventListener('change', reload);
    document.getElementById('filterMyAdminActionFilter').addEventListener('change', reload);
    document.getElementById('resetMyFilters').addEventListener('click', function () {
        document.getElementById('filterMyGameType').value = '';
        document.getElementById('filterMyAdminActionFilter').value = '';
        reload();
    });

    let selectedRowId = null;

    $('#myActionsTable tbody').on('click', 'tr', function () {
        const data = dt.row(this).data();
        if (!data) return;
        selectedRowId = data.id;
        $('#myActionsTable tbody tr').removeClass('my-action-selected');
        $(this).addClass('my-action-selected');
        loadDetails(selectedRowId);
    });

    function loadDetails(id) {
        const panel = document.getElementById('myActionDetailsPanel');
        panel.innerHTML = '<div class="p-3 text-center text-muted"><span class="spinner-border spinner-border-sm"></span> Loading...</div>';
        fetch('/AdminActions/GetMyAdminActionDetails?id=' + encodeURIComponent(id))
            .then(r => { if (!r.ok) throw new Error('Failed'); return r.text(); })
            .then(html => { panel.innerHTML = html; })
            .catch(() => { panel.innerHTML = '<div class="alert alert-danger m-2">Failed to load details.</div>'; });
    }

    function updateScrollOnly() {
        const newY = calcScrollY();
        const settings = dt.settings()[0];
        settings.oScroll.sY = newY;
        const scrollBody = tableEl.closest('.dataTables_wrapper')?.querySelector('.dataTables_scrollBody');
        if (scrollBody) scrollBody.style.height = newY;
    }

    window.addEventListener('resize', updateScrollOnly);

    // Drag resize between panels
    const layoutRoot = document.getElementById('myActionsLayoutRoot');
    const resizer = document.getElementById('myActionsResizer');
    const leftPane = layoutRoot?.querySelector('.my-actions-left');
    const storageKey = 'myActions.leftWidth';
    const min = 320; // sync with CSS var
    const max = 900;

    function applyWidth(px) {
        const clamped = Math.min(max, Math.max(min, px));
        layoutRoot.style.setProperty('--my-actions-left-width', clamped + 'px');
        localStorage.setItem(storageKey, String(clamped));
        // Recompute scroll height only (no Ajax redraw)
        setTimeout(updateScrollOnly, 16);
    }

    // Initialize from persisted width
    const saved = parseInt(localStorage.getItem(storageKey), 10);
    if (!isNaN(saved)) {
        applyWidth(saved);
    }

    if (resizer && leftPane && layoutRoot) {
        let dragging = false;
        let startX = 0;
        let startWidth = 0;

        function pointerDown(e) {
            dragging = true;
            startX = e.clientX;
            startWidth = leftPane.getBoundingClientRect().width;
            resizer.classList.add('dragging');
            document.addEventListener('pointermove', pointerMove);
            document.addEventListener('pointerup', pointerUp, { once: true });
            e.preventDefault();
        }
        function pointerMove(e) {
            if (!dragging) return;
            const delta = e.clientX - startX;
            applyWidth(startWidth + delta);
        }
        function pointerUp() {
            dragging = false;
            resizer.classList.remove('dragging');
            document.removeEventListener('pointermove', pointerMove);
        }
        resizer.addEventListener('pointerdown', pointerDown);

        // Keyboard accessibility (left/right arrows)
        resizer.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
                const current = leftPane.getBoundingClientRect().width;
                const delta = (e.key === 'ArrowLeft' ? -20 : 20);
                applyWidth(current + delta);
                e.preventDefault();
            }
            if (e.key === 'Home') { applyWidth(min); e.preventDefault(); }
            if (e.key === 'End') { applyWidth(max); e.preventDefault(); }
        });
    }
})();
