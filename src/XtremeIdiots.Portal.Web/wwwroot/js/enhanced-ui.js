// XtremeIdiots Portal - Enhanced UI Interactions

// Wait for the document to be ready
$(document).ready(function () {
    // Add tooltips to all elements with title attributes
    $('[title]').tooltip();

    // Add confirmation dialogs to delete buttons
    $('.btn-danger[data-confirm]').on('click', function (e) {
        var message = $(this).data('confirm') || 'Are you sure you want to perform this action?';
        if (!confirm(message)) {
            e.preventDefault();
        }
    });

    // Make non-DataTables tables responsive (avoid interfering with DataTables width calculations)
    $('.table').not('.dataTable').not('#dataTable').each(function () {
        var $t = $(this);
        if (!$t.parent().hasClass('table-responsive')) {
            $t.wrap('<div class="table-responsive"></div>');
        }
    });

    // Enhance DataTables if they exist
    if ($.fn.dataTable) {
        $('.dataTable').each(function () {
            var $table = $(this);
            if (!$.fn.DataTable.isDataTable($table)) {
                $table.DataTable({
                    responsive: true,
                    language: {
                        search: '<i class="fa fa-search"></i>',
                        lengthMenu: 'Show _MENU_ entries',
                        info: 'Showing _START_ to _END_ of _TOTAL_ entries'
                    },
                    dom: '<"top"flp>rt<"bottom"ip>',
                    pageLength: 25
                });
            }
        });
    }

    // Add ripple effect to buttons
    $('.btn').on('click', function (e) {
        var $btn = $(this);

        // Remove existing ripple elements
        $btn.find('.ripple').remove();

        // Create new ripple element
        var $ripple = $('<span class="ripple"></span>');
        $btn.append($ripple);

        // Set ripple position based on click position
        var btnOffset = $btn.offset();
        var xPos = e.pageX - btnOffset.left;
        var yPos = e.pageY - btnOffset.top;

        $ripple.css({
            top: yPos + 'px',
            left: xPos + 'px'
        }).addClass('animate');

        // Remove ripple after animation
        setTimeout(function () {
            $ripple.remove();
        }, 600);
    });

    // Add form validation styling
    $('form').on('submit', function () {
        $(this).addClass('was-validated');
    });

    // Fix navigation menu functionality
    ensureExpandedParents();
    enableMiniNavbarPopouts();

    // Add animation to alerts
    $('.alert').addClass('animated fadeIn');

    // Ensure the current navigation hierarchy is expanded for active items
    function ensureExpandedParents() {
        $('.nav-second-level li.active').each(function () {
            const $submenu = $(this).closest('ul.nav-second-level');
            if ($submenu.length === 0) {
                return;
            }

            $submenu
                .addClass('show')
                .attr('aria-expanded', 'true');

            const $parentItem = $submenu.parent('li');
            $parentItem.addClass('active');
            $parentItem.children('a').attr('aria-expanded', 'true');
        });
    }

    function enableMiniNavbarPopouts() {
        const $body = $('body');
        const $menu = $('#side-menu');
        if ($menu.length === 0) {
            return;
        }

        const submenuSelector = '> li';

        function showSubmenu($item) {
            if (!$body.hasClass('mini-navbar')) {
                return;
            }

            const $submenu = $item.children('ul.nav-second-level');
            if ($submenu.length === 0) {
                return;
            }

            const wasOpen = $submenu.hasClass('show');
            $submenu.data('was-open', wasOpen);

            $item.addClass('mini-open');
            if (!wasOpen) {
                $submenu.addClass('show');
            }

            $submenu
                .addClass('mini-force-open')
                .attr('aria-expanded', 'true')
                .css('display', 'block');

            $item.children('a').attr('aria-expanded', 'true');
        }

        function hideSubmenu($item) {
            if (!$body.hasClass('mini-navbar')) {
                return;
            }

            const $submenu = $item.children('ul.nav-second-level');
            if ($submenu.length === 0) {
                return;
            }

            const wasOpen = $submenu.data('was-open');

            $item.removeClass('mini-open');
            $submenu.removeClass('mini-force-open');
            $submenu.removeAttr('style');

            if (!wasOpen) {
                $submenu.removeClass('show');
            }

            $submenu.attr('aria-expanded', wasOpen ? 'true' : 'false');
            $item.children('a').attr('aria-expanded', wasOpen ? 'true' : 'false');
            $submenu.removeData('was-open');
        }

        $menu.on('mouseenter', submenuSelector, function () {
            showSubmenu($(this));
        });

        $menu.on('mouseleave', submenuSelector, function () {
            hideSubmenu($(this));
        });

        $menu.on('focusin', submenuSelector, function () {
            showSubmenu($(this));
        });

        $menu.on('focusout', submenuSelector, function () {
            const $item = $(this);
            setTimeout(function () {
                if ($item.find(document.activeElement).length > 0) {
                    return;
                }
                hideSubmenu($item);
            }, 0);
        });
    }
});

// Add ripple CSS style
(function () {
    var style = document.createElement('style');
    style.type = 'text/css';
    style.innerHTML = `
        .btn {
            position: relative;
            overflow: hidden;
        }
        .ripple {
            position: absolute;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.4);
            transform: scale(0);
            width: 100px;
            height: 100px;
            margin-top: -50px;
            margin-left: -50px;
            pointer-events: none;
        }
        .ripple.animate {
            animation: ripple-animation 0.6s linear;
        }
        @keyframes ripple-animation {
            0% {
                transform: scale(0);
                opacity: 0.5;
            }
            100% {
                transform: scale(3);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);
})();
