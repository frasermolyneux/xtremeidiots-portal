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
    });    // Fix navigation menu functionality
    fixNavigationMenu();

    // Add animation to alerts
    $('.alert').addClass('animated fadeIn');    // Function to fix menu navigation issues
    function fixNavigationMenu() {
        // First, ensure all active items have their parent menus expanded
        $('.nav-second-level li.active').each(function () {
            // Find the parent menu container
            var $parentMenu = $(this).closest('ul.nav-second-level');

            // Make sure the submenu is shown
            if ($parentMenu.hasClass('collapse') && !$parentMenu.hasClass('show')) {
                $parentMenu.addClass('show');
            }
        });

        // Ensure all menu items with submenu can be clicked to expand
        $('.metismenu > li > a').each(function () {
            var $link = $(this);
            var $arrow = $link.find('.fa.arrow');

            if ($arrow.length) {
                $link.off('click.menuToggle').on('click.menuToggle', function (e) {
                    e.preventDefault();
                    var $parentLi = $link.parent('li');
                    var $submenu = $parentLi.find('> ul.nav-second-level');

                    // Toggle the submenu
                    if ($submenu.hasClass('show')) {
                        // Close submenu
                        $submenu.removeClass('show');
                        // Don't remove active class as it should be controlled by the IsSelected method
                    } else {
                        // Open submenu
                        $submenu.addClass('show');
                        // Don't add active class as it should be controlled by the IsSelected method

                        // Close other open submenus at the same level (optional)
                        $parentLi.siblings().find('> ul.nav-second-level.show').removeClass('show');
                    }
                });
            }
        });
    }

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        $('.alert:not(.alert-important)').fadeOut(500, function () {
            $(this).remove();
        });
    }, 5000);
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
