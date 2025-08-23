(function (global, $) {
    function initUserSearchAutocomplete(options) {
        const cfg = Object.assign({ minLength: 2, delay: 300 }, options || {});
        const $input = $(cfg.inputSelector);
        if ($input.length === 0) return;
        const $hidden = $(cfg.hiddenSelector);
        const $container = $('<div class="user-suggestions list-group position-absolute bg-white shadow" role="listbox" aria-label="User suggestions" style="z-index:1050; max-height:240px; overflow-y:auto; display:none;"></div>');
        $input.after($container);
        let timer = null, lastQuery = '';

        function clearSuggestions() { $container.hide().empty(); }
        function selectItem($item) {
            $input.val($item.text());
            $hidden.val($item.data('value'));
            clearSuggestions();
            if (typeof cfg.onSelect === 'function') cfg.onSelect($item.data('value'));
        }
        function search(term) {
            if (!term || term.length < cfg.minLength) { clearSuggestions(); return; }
            if (term === lastQuery) return;
            lastQuery = term;
            fetch(cfg.searchUrl + '?term=' + encodeURIComponent(term))
                .then(r => r.json())
                .then(results => {
                    $container.empty();
                    if (!Array.isArray(results) || results.length === 0) { clearSuggestions(); return; }
                    results.forEach((r, i) => {
                        const $item = $('<button type="button" class="list-group-item list-group-item-action py-1 px-2" role="option"></button>');
                        $item.attr('id', 'user-sugg-' + i).attr('data-value', r.id).text(r.text);
                        $item.on('click', () => selectItem($item));
                        $container.append($item);
                    });
                    $container.css({ width: $input.outerWidth(), position: 'absolute' }).show();
                })
                .catch(() => clearSuggestions());
        }
        $input.on('input', function () {
            $hidden.val('');
            clearTimeout(timer);
            const term = $(this).val().trim();
            timer = setTimeout(() => search(term), cfg.delay);
        });
        $input.on('keydown', function (e) {
            if (!$container.is(':visible')) return;
            const $items = $container.find('.list-group-item');
            if ($items.length === 0) return;
            let idx = $items.index($items.filter('.active'));
            if (e.key === 'ArrowDown') { e.preventDefault(); idx = (idx + 1) % $items.length; }
            else if (e.key === 'ArrowUp') { e.preventDefault(); idx = (idx <= 0 ? $items.length - 1 : idx - 1); }
            else if (e.key === 'Enter') { e.preventDefault(); if (idx >= 0) selectItem($items.eq(idx)); return; }
            else if (e.key === 'Escape') { clearSuggestions(); return; }
            else return;
            $items.removeClass('active').attr('aria-selected', 'false');
            const $focus = $items.eq(idx).addClass('active').attr('aria-selected', 'true');
            $focus[0].scrollIntoView({ block: 'nearest' });
        });
        $(document).on('click.userSearch', function (e) {
            if (!$(e.target).closest(cfg.inputSelector).length && !$(e.target).closest('.user-suggestions').length) { clearSuggestions(); }
        });
    }
    global.initUserSearchAutocomplete = initUserSearchAutocomplete;
})(window, jQuery);
