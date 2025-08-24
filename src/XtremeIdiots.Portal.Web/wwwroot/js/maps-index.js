// Maps Index page script extracted from inline Razor view
$(document).ready(function () {
    const tableEl = $('#dataTable');
    const dataUrl = tableEl.data('source');

    const table = tableEl.DataTable({
        processing: true,
        serverSide: true,
        searchDelay: 800,
        stateSave: true,
        responsive: { details: { type: 'inline', target: 'tr' } },
        autoWidth: false,
        order: [[1, 'asc']],
        columnDefs: [
            { targets: 0, responsivePriority: 2 }, // Game
            { targets: 1, responsivePriority: 1 }, // Name
            { targets: 2, responsivePriority: 5 }, // Map Files
            { targets: 3, responsivePriority: 3 }, // Popularity
            { targets: 4, responsivePriority: 4 }  // Image
        ],
        ajax: {
            url: dataUrl,
            dataSrc: 'data',
            contentType: 'application/json',
            type: 'POST',
            data: function (d) { return JSON.stringify(d); },
            beforeSend: function (xhr) {
                const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenInput) {
                    xhr.setRequestHeader('RequestVerificationToken', tokenInput.value);
                }
                // mutate url based on filters
                const gt = document.getElementById('filterGameType')?.value;
                let base = dataUrl;
                if (gt) {
                    base = dataUrl.replace(/\/Maps\/GetMapListAjax.*/, '/Maps/GetMapListAjax/' + encodeURIComponent(gt));
                }
                this.url = base;
            }
        },
        columns: [
            { data: 'gameType', name: 'gameType', sortable: true, render: function (data) { return gameTypeIcon(data); } },
            { data: 'mapName', name: 'mapName', sortable: true },
            { data: 'mapFiles', name: 'mapFiles', sortable: false, render: renderMapFiles },
            { data: null, name: 'popularity', sortable: true, render: renderPopularity },
            { data: null, sortable: false, render: renderImage }
        ]
    });

    // Move the built-in DataTables search box into our custom filters bar for unified UX
    (function relocateSearch() {
        const filters = document.getElementById('mapsFilters');
        const dtFilter = document.getElementById('dataTable_filter'); // DataTables generates this
        if (!filters || !dtFilter) return;

        dtFilter.classList.add('filter-group');
        const label = dtFilter.querySelector('label');
        if (label) {
            const input = label.querySelector('input');
            if (input) {
                input.classList.add('form-control');
                input.placeholder = 'Search maps...';
                label.textContent = '';
                const newLabel = document.createElement('label');
                newLabel.className = 'form-label';
                newLabel.setAttribute('for', input.id || 'globalMapsSearch');
                if (!input.id) input.id = 'globalMapsSearch';
                newLabel.textContent = 'Search';
                dtFilter.appendChild(newLabel);
                dtFilter.appendChild(input);
            }
        }
        // Place search before the reset (clear filters) group so the clear button is last
        const resetBtn = document.getElementById('resetFilters');
        const resetGroup = resetBtn ? resetBtn.closest('.filter-group') : null;
        if (resetGroup && resetGroup.parentElement === filters) {
            filters.insertBefore(dtFilter, resetGroup);
        } else {
            // fallback
            filters.appendChild(dtFilter);
        }
    })();

    function renderMapFiles(data) {
        if (!data || !data.length) return '<span class="text-muted">No Map Files</span>';
        let html = '<ul class="list-unstyled mb-0 map-files">';
        for (const f of data) {
            if (f && f.url && f.fileName) {
                html += '<li><a href="' + f.url + '">' + f.fileName + '</a></li>';
            }
        }
        html += '</ul>';
        return html;
    }

    function renderPopularity(data, type, row) {
        let likePct = row.likePercentage || 0;
        let dislikePct = row.dislikePercentage || 0;
        const totalLikes = row.totalLikes || 0;
        const totalDislikes = row.totalDislikes || 0;
        const totalVotes = row.totalVotes || 0;

        // If API returns values 0-1 treat as proportions
        if (likePct <= 1 && dislikePct <= 1 && (likePct + dislikePct) <= 2) {
            likePct *= 100;
            dislikePct *= 100;
        }
        // Derive if counts exist but percentages zero
        if (totalVotes > 0 && likePct === 0 && dislikePct === 0 && (totalLikes > 0 || totalDislikes > 0)) {
            likePct = totalLikes / totalVotes * 100;
            dislikePct = totalDislikes / totalVotes * 100;
        }
        // Ensure tiny bars show
        if (likePct > 0 && likePct < 1) likePct = 1;
        if (dislikePct > 0 && dislikePct < 1) dislikePct = 1;

        const likePctStr = likePct.toFixed(2);
        const dislikePctStr = dislikePct.toFixed(2);
        return '<div class="progress" id="progress-' + row.mapName + '" style="width:100%;min-width:140px;max-width:260px;">' +
            '<div class="progress-bar bg-info" role="progressbar" style="width: ' + likePctStr + '%" aria-valuenow="' + totalLikes + '" aria-valuemin="0" aria-valuemax="' + totalVotes + '"></div>' +
            '<div class="progress-bar bg-danger" role="progressbar" style="width: ' + dislikePctStr + '%" aria-valuenow="' + totalDislikes + '" aria-valuemin="0" aria-valuemax="' + totalVotes + '"></div>' +
            '</div>' +
            '<div class="m-t-sm">' + totalLikes + ' likes and ' + totalDislikes + ' dislikes out of ' + totalVotes + '</div>';
    }

    function renderImage(data, type, row) {
        const src = row.mapImageUri || '/images/noimage.jpg';
        const alt = row.mapImageUri ? ('Map image for ' + row.mapName) : 'No map image available';
        return '<img class="map-image img-fluid" alt="' + alt + '" src="' + src + '" loading="lazy" />';
    }

    function applyGameColumnVisibility() {
        const hasSpecificGame = document.getElementById('filterGameType')?.value !== '';
        table.column(0).visible(!hasSpecificGame, false);
    }

    document.getElementById('filterGameType')?.addEventListener('change', function () {
        applyGameColumnVisibility();
        table.ajax.reload(null, false);
    });

    document.getElementById('resetFilters')?.addEventListener('click', function () {
        const sel = document.getElementById('filterGameType');
        let changed = false;
        if (sel && sel.value !== '') {
            sel.value = '';
            applyGameColumnVisibility();
            changed = true;
        }
        // Clear global search box
        if (table.search()) {
            table.search('');
            changed = true;
        }
        // Always reset to first page when clearing filters/search
        if (changed) {
            table.page('first');
        }
        table.draw(false);
    });

    applyGameColumnVisibility();

    // Reduce internal padding under the table region
    const iboxContent = tableEl.closest('.ibox-content');
    if (iboxContent) {
        iboxContent.classList.add('datatable-tight');
    }
});
