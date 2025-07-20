---
description: 'Guidelines for building JavaScript code in the XtremeIdiots Portal'
applyTo: '**/*.js'
---

# JavaScript Development Guidelines

> **IMPORTANT**: Follow modern JavaScript best practices while maintaining compatibility with the existing jQuery-based infrastructure.

## Core Principles

**Write clean, maintainable, and secure JavaScript code that enhances the gaming community portal experience while following established patterns.**

## Code Structure and Organization

### File Organization
```
wwwroot/js/
├── site.js              # Core portal functionality and utilities
├── enhanced-ui.js       # UI enhancements and interactions
├── inspinia.js          # Theme-specific functionality
└── components/          # Reusable component scripts (future)
```

### Script Loading Pattern
Use environment-specific loading in Razor views:
```html
@section Scripts {
    <environment names="Development">
        <script src="~/js/site.js"></script>
    </environment>
    <environment names="Staging,Production">
        <script src="~/js/site.min.js"></script>
    </environment>
}
```

## JavaScript Standards

### Modern Syntax and Features
- **Use ES6+ features** where supported: `const`, `let`, arrow functions, template literals
- **Prefer const/let** over `var` for variable declarations
- **Use template literals** for string interpolation: `` `Hello ${name}` ``
- **Use arrow functions** for callbacks and short functions
- **Use destructuring** for cleaner object/array access

### jQuery Integration
Since the project uses jQuery extensively:
- **Wrap in document ready**: Always use `$(document).ready(function() { ... })`
- **Cache jQuery selectors**: Store frequently used selectors in variables
- **Use proper event binding**: Use `.on()` for event delegation
- **Namespace events**: Use event namespaces for easier removal (e.g., `click.myModule`)

```javascript
// ✅ Good: Modern jQuery patterns
$(document).ready(function () {
    const $dataTable = $('#dataTable');
    const $confirmBtns = $('.btn-danger[data-confirm]');
    
    $confirmBtns.on('click.confirmDialog', function (e) {
        const message = $(this).data('confirm') || 'Are you sure?';
        if (!confirm(message)) {
            e.preventDefault();
        }
    });
});
```

## Security Best Practices

### XSS Prevention
**ALWAYS** escape user input before inserting into DOM:
```javascript
function escapeHtml(unsafe) {
    return unsafe
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

// ✅ Good: Escaped content
function renderPlayerName(gameType, username, playerId) {
    const safeUsername = escapeHtml(username);
    return `${gameTypeIconEnum(gameType)} <a href='/Players/Details/${playerId}'>${safeUsername}</a>`;
}
```

### CSRF Protection
Include anti-forgery tokens in AJAX requests:
```javascript
// Add to request headers
beforeSend: function (xhr) {
    xhr.setRequestHeader("RequestVerificationToken", 
        $('input[name="__RequestVerificationToken"]').val());
}
```

## Gaming-Specific Patterns

### Game Type Handling
Use consistent game type utilities:
```javascript
function gameTypeIconEnum(gameType) {
    const gameTypeMap = {
        1: "CallOfDuty2",
        2: "CallOfDuty4", 
        3: "CallOfDuty5",
        4: "Insurgency"
    };
    return gameTypeMap[gameType] || "Unknown";
}

function gameTypeIcon(gameType) {
    const gameTypeName = gameTypeIconEnum(gameType);
    return `<img src='/images/game-icons/${gameTypeName}.png' alt='${gameTypeName}' width='16' height='16' />`;
}
```

### Player Data Rendering
Consistent player information display:
```javascript
function renderPlayerName(gameType, username, playerId = null) {
    const safeUsername = escapeHtml(username);
    const icon = gameTypeIconEnum(gameType);
    
    if (playerId) {
        return `${icon} <a href='/Players/Details/${playerId}'>${safeUsername}</a>`;
    }
    return `${icon} ${safeUsername}`;
}
```

## DataTables Integration

### Standard Configuration
Use consistent DataTables setup:
```javascript
$('#dataTable').DataTable({
    processing: true,
    serverSide: true,
    searchDelay: 1000,
    stateSave: true,
    responsive: true,
    language: {
        search: '<i class="fa fa-search" aria-hidden="true"></i>',
        lengthMenu: 'Show _MENU_ entries',
        info: 'Showing _START_ to _END_ of _TOTAL_ entries'
    },
    dom: '<"top"flp>rt<"bottom"ip>',
    pageLength: 25,
    ajax: {
        url: dataUrl,
        dataSrc: 'data',
        contentType: "application/json",
        type: "POST",
        data: function (d) {
            return JSON.stringify(d);
        }
    }
});
```

### Custom Renderers
Create reusable column renderers:
```javascript
const columnRenderers = {
    playerName: function (data, type, row) {
        return renderPlayerName(row.gameType, row.username, row.playerId);
    },
    
    dateTime: function (data, type, row) {
        if (type === 'display' && data) {
            return moment(data).format('YYYY-MM-DD HH:mm:ss');
        }
        return data;
    },
    
    actions: function (data, type, row) {
        return `
            <a href='/Controller/Details/${row.id}' class='btn btn-sm btn-outline-primary'>
                <i class='fa fa-eye' aria-hidden='true'></i>
            </a>
        `;
    }
};
```

## Error Handling

### AJAX Error Handling
Implement consistent error handling:
```javascript
function handleAjaxError(xhr, textStatus, errorThrown) {
    console.error('AJAX Error:', {
        status: xhr.status,
        statusText: xhr.statusText,
        responseText: xhr.responseText,
        textStatus,
        errorThrown
    });
    
    // Show user-friendly message
    toastr.error('An error occurred while processing your request. Please try again.');
}

// Use in AJAX calls
$.ajax({
    // ... other options
    error: handleAjaxError
});
```

### Form Validation
Enhance client-side validation:
```javascript
function validateForm($form) {
    let isValid = true;
    const errors = [];
    
    // Custom validation logic
    $form.find('input[required]').each(function () {
        const $input = $(this);
        if (!$input.val().trim()) {
            isValid = false;
            errors.push(`${$input.attr('name')} is required`);
            $input.addClass('is-invalid');
        } else {
            $input.removeClass('is-invalid');
        }
    });
    
    if (!isValid) {
        toastr.error('Please fix the following errors: ' + errors.join(', '));
    }
    
    return isValid;
}
```

## Performance Best Practices

### Efficient DOM Manipulation
- **Cache selectors**: Don't query the same elements repeatedly
- **Batch DOM updates**: Use DocumentFragment or detached elements for multiple changes
- **Debounce expensive operations**: Use setTimeout for search inputs, resize handlers

```javascript
// ✅ Good: Cached selectors and debounced search
const $searchInput = $('#search');
let searchTimeout;

$searchInput.on('input', function () {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        performSearch($(this).val());
    }, 300);
});
```

### Memory Management
- **Remove event listeners**: Use `.off()` when removing elements
- **Clear intervals/timeouts**: Store references and clear them
- **Avoid memory leaks**: Don't create circular references with closures

## Accessibility Standards

### ARIA Support
Add appropriate ARIA attributes:
```javascript
// Add ARIA labels to dynamic content
function createButton(text, action) {
    return `
        <button type="button" 
                class="btn btn-primary" 
                aria-label="${text}"
                onclick="${action}">
            <i class="fa fa-plus" aria-hidden="true"></i> ${text}
        </button>
    `;
}
```

### Keyboard Navigation
Ensure keyboard accessibility:
```javascript
// Handle keyboard events for custom components
$('.custom-dropdown').on('keydown', function (e) {
    switch (e.key) {
        case 'Enter':
        case ' ':
            $(this).trigger('click');
            e.preventDefault();
            break;
        case 'Escape':
            $(this).blur();
            break;
    }
});
```

## Notification Patterns

### Toastr Integration
Use consistent notification styling:
```javascript
// Success notifications
function showSuccess(message) {
    toastr.success(message, 'Success', {
        timeOut: 3000,
        positionClass: 'toast-top-right'
    });
}

// Error notifications  
function showError(message) {
    toastr.error(message, 'Error', {
        timeOut: 5000,
        positionClass: 'toast-top-right'
    });
}

// Info notifications
function showInfo(message) {
    toastr.info(message, 'Information', {
        timeOut: 4000,
        positionClass: 'toast-top-right'
    });
}
```

## Code Documentation

### Function Documentation
Document public functions with JSDoc:
```javascript
/**
 * Renders a player name with game type icon and optional link
 * @param {number} gameType - The game type enum value
 * @param {string} username - The player's username
 * @param {string|null} playerId - Optional player ID for linking
 * @returns {string} HTML string with formatted player name
 */
function renderPlayerName(gameType, username, playerId = null) {
    // Implementation
}
```

### Inline Comments
Only comment complex business logic:
```javascript
// Calculate progressive tax brackets for admin action severity
const severity = calculateProgressiveScale(violations, [1.0, 1.5, 2.0], [5, 10]);
```

## Browser Compatibility

### Support Requirements
- **Modern browsers**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **Graceful degradation**: Core functionality must work without JavaScript
- **Progressive enhancement**: JavaScript should enhance, not replace, basic functionality

### Polyfills and Fallbacks
```javascript
// Feature detection before usage
if (typeof Promise !== 'undefined') {
    // Use modern Promise-based code
} else {
    // Fallback to callbacks or library
}
```

## Anti-Patterns to Avoid

### ❌ Bad Practices
```javascript
// Don't use var in modern code
var name = 'player';

// Don't use innerHTML with unsanitized data
element.innerHTML = userInput;

// Don't use eval() ever
eval(userCode);

// Don't create global variables accidentally
function myFunction() {
    leakedGlobal = 'oops'; // Missing var/let/const
}

// Don't ignore errors silently
try {
    riskyOperation();
} catch (e) {
    // Silent failure
}
```

### ✅ Good Practices
```javascript
// Use const/let appropriately
const playerName = 'username';
let currentScore = 0;

// Sanitize before inserting
element.textContent = escapeHtml(userInput);

// Handle errors appropriately
try {
    riskyOperation();
} catch (error) {
    console.error('Operation failed:', error);
    showError('Unable to complete operation');
}
```

## Testing Considerations

### Manual Testing
- **Cross-browser testing**: Verify functionality in all supported browsers
- **Mobile responsiveness**: Test on various device sizes
- **Accessibility testing**: Use screen readers and keyboard-only navigation
- **Performance testing**: Monitor for memory leaks and slow operations

### Error Scenarios
- **Network failures**: Test AJAX error handling
- **Invalid data**: Test with malformed server responses
- **User edge cases**: Test with empty inputs, special characters, very long text

## Summary

Write JavaScript that enhances the gaming portal experience while maintaining security, performance, and accessibility standards. Follow established jQuery patterns while adopting modern JavaScript features where appropriate. Always prioritize user safety through proper input sanitization and error handling.
