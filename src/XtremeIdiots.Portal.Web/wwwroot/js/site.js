function escapeHtml(unsafe) {
    return unsafe
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function handleAjaxError(xhr, textStatus, error) {
    console.log(textStatus);
}

function renderPlayerName(gameType, username) {
    var safeUsername = escapeHtml(username);
    return gameTypeIconEnum(gameType) + " " + safeUsername;
}

function renderPlayerName(gameType, username, playerId) {
    var safeUsername = escapeHtml(username);
    return gameTypeIconEnum(gameType) + " <a href='/Players/Details/" + playerId + "'>" + safeUsername + "</a>";
}

function chatLogUrl(chatMessageId) {
    return "<a href='/ServerAdmin/ChatLogPermaLink/" + chatMessageId + "'>PermLink</a>";
}

function gameTypeIcon(gameType) {
    return "<img src='/images/game-icons/" +
        gameType +
        ".png' alt='" +
        gameType +
        "' width='16' height='16' />";
}

function gameTypeIconEnum(gameType) {
    var gameTypeString = "Unknown";

    switch (gameType) {
        case 1:
            gameTypeString = "CallOfDuty2";
            break;
        case 2:
            gameTypeString = "CallOfDuty4";
            break;
        case 3:
            gameTypeString = "CallOfDuty5";
            break;
    }

    return "<img src='/images/game-icons/" +
        gameTypeString +
        ".png' alt='" +
        gameTypeString +
        "' width='16' height='16' />";
}

// Returns a font-awesome icon followed by the admin action type text.
// Mirrors mappings used in server-side views (AdminActions ViewComponent).
function adminActionTypeIcon(actionType) {
    if (!actionType) return '';
    var iconClass = 'fa fa-question-circle';
    switch (actionType) {
        case 'Observation':
            iconClass = 'fa fa-eye';
            break;
        case 'Warning':
            iconClass = 'fa fa-exclamation-triangle';
            break;
        case 'Kick':
            iconClass = 'fa fa-user-times';
            break;
        case 'TempBan':
            iconClass = 'fa fa-clock-o';
            break;
        case 'Ban':
            iconClass = 'fa fa-ban';
            break;
    }
    return "<i class='" + iconClass + "' aria-hidden='true'></i> <span class='action-text'>" + actionType + "</span>";
}

// Global DataTables footer spacing helper: ensures wrapper gets padding to avoid footer overlap.
(function () {
    function applyDataTableSpacing() {
        var wrapper = document.querySelector('.wrapper.wrapper-content');
        if (!wrapper) return;
        if (document.querySelector('.dataTables_wrapper')) {
            wrapper.classList.add('with-datatable');
        }
    }

    // Run after DOM ready with slight delay (allow DataTables init).
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { setTimeout(applyDataTableSpacing, 400); });
    } else {
        setTimeout(applyDataTableSpacing, 400);
    }

    // Observe for late-injected tables (AJAX navigation or deferred init).
    var observer = new MutationObserver(function (mutations) {
        for (var i = 0; i < mutations.length; i++) {
            if (mutations[i].addedNodes && mutations[i].addedNodes.length) {
                if (document.querySelector('.dataTables_wrapper')) {
                    applyDataTableSpacing();
                    observer.disconnect();
                    break;
                }
            }
        }
    });
    try {
        observer.observe(document.body, { childList: true, subtree: true });
    } catch (e) {
        // Silently ignore if observe fails (very old browsers)
    }
})();

function downloadDemoLink(demoName, demoId) {
    return "<a href='/Demos/Download/" + demoId + "'>" + demoName + "</a>";
}

function deleteDemoLink(demoId, gameType = null) {
    if (gameType === null) {
        return '<div class="btn-group btn-group-sm" role="group">' +
            '<a type="button" class="btn btn-danger"  href="/Demos/Delete/' +
            demoId +
            '"><i class="fa fa-trash"></i> Delete Demo</a>' +
            "</div>";
    } else {
        return '<div class="btn-group btn-group-sm" role="group">' +
            '<a type="button" class="btn btn-danger"  href="/Demos/Delete/' +
            demoId +
            '?filterGame=true"><i class="fa fa-trash"></i> Delete Demo</a>' +
            "</div>";
    }
}

function geoLocationIpLink(ipAddress) {
    return "<a href='https://www.geo-location.net/Home/LookupAddress/" + ipAddress + "'>" + ipAddress + "</a>";
}

/**
 * Formats an IP address with consistent display including:
 * {country flag} {IP Address} {Risk Pill} {Type Pill} {VPN/Proxy Pills}
 * 
 * @param {string} ipAddress - The IP address to format
 * @param {number} riskScore - The risk score (0-100) from ProxyCheck
 * @param {boolean} isProxy - Whether the IP is a proxy
 * @param {boolean} isVpn - Whether the IP is a VPN
 * @param {string} type - The type of proxy/VPN
 * @param {string} countryCode - Optional country code for the flag
 * @param {boolean} linkToDetails - Whether to link the IP to the details page (default: true)
 * @returns {string} HTML formatted IP address
 */
function formatIPAddress(ipAddress, riskScore, isProxy, isVpn, type = '', countryCode = '', linkToDetails = true) {
    if (!ipAddress) return '';

    let result = '';

    // 1. Country Flag
    if (countryCode && countryCode !== '') {
        result += "<img src='/images/flags/" + countryCode.toLowerCase() + ".png' /> ";
    } else {
        result += "<img src='/images/flags/unknown.png' /> ";
    }

    // 2. IP Address (with or without link)
    if (linkToDetails) {
        result += "<a href='/IPAddresses/Details?ipAddress=" + ipAddress + "'>" + ipAddress + "</a> ";
    } else {
        result += ipAddress + " ";
    }

    // 3. Risk Score Pill
    if (riskScore !== null && riskScore !== undefined) {
        let riskClass = 'text-bg-success';
        if (riskScore >= 80) {
            riskClass = 'text-bg-danger';
        } else if (riskScore >= 50) {
            riskClass = 'text-bg-warning';
        } else if (riskScore >= 25) {
            riskClass = 'text-bg-info';
        }

        result += "<span class='badge rounded-pill " + riskClass + "'>Risk: " + riskScore + "</span> ";
    }

    // 4. Type Pill
    if (type && type !== '') {
        result += "<span class='badge rounded-pill text-bg-primary'>" + type + "</span> ";
    }

    // 5. Proxy Pill
    if (isProxy) {
        result += "<span class='badge rounded-pill text-bg-danger'>Proxy</span> ";
    }

    // 6. VPN Pill
    if (isVpn) {
        result += "<span class='badge rounded-pill text-bg-warning'>VPN</span>";
    }

    return result;
}

function manageClaimsLink(userId) {
    return '<div class="btn-group btn-group-sm" role="group">' +
        '<a type="button" class="btn btn-primary"  href="/User/ManageProfile/' +
        userId +
        '?filterGame=true"><i class="fa fa-key"></i> Manage Claims</a>' +
        "</div>";
}

function logOutUserLink(id, antiForgeryToken) {
    return '<form class="form-inline" method="post" action="/User/LogUserOut" method="post">' +
        '<input id="id" name="id" type="hidden" value="' +
        id +
        '\"/>' +
        '<button class="btn btn-primary" type="submit">Logout User</button>' +
        antiForgeryToken +
        '</form>';
}

// Formats a UTC date string (YYYY-MM-DD HH:mm) to a relative "time ago" string using moment.js.
// Falls back to original string if moment isn't available.
function timeAgo(utcYmdHm) {
    if (!utcYmdHm) return '';
    if (typeof moment !== 'undefined') {
        // Expecting server sends "YYYY-MM-DD HH:mm" (UTC)
        var m = moment.utc(utcYmdHm, 'YYYY-MM-DD HH:mm');
        if (!m.isValid()) return utcYmdHm;
        return m.fromNow();
    }
    return utcYmdHm; // fallback
}

// Formats an expiry datetime (YYYY-MM-DD HH:mm) as a date string in the user's locale (day month year).
// Falls back to 'Never' if value indicates no expiry or original string if parsing fails.
function formatExpiryDate(utcYmdHm) {
    if (!utcYmdHm) return '';
    if (utcYmdHm === 'Never') return 'Never';
    var locale = document.querySelector('meta[name="user-locale"]')?.content || 'en';
    var tz = document.querySelector('meta[name="user-timezone"]')?.content; // e.g., Europe/London
    if (typeof moment !== 'undefined') {
        var m = moment.utc(utcYmdHm, 'YYYY-MM-DD HH:mm');
        if (!m.isValid()) return utcYmdHm;
        if (moment.locale) moment.locale(locale);
        var display = (tz && moment.tz) ? m.tz(tz) : m.local();
        var dateStr = display.format('LL');
        var expired = m.isBefore(moment.utc());
        if (expired) {
            // Badge style for expired
            return "<span title='Expired on " + dateStr + "'>" + dateStr + " <span class='badge text-bg-danger ms-1'>Expired</span></span>";
        }
        // Active (not yet expired)
        return "<span title='Expires on " + dateStr + "'>" + dateStr + " <span class='badge text-bg-success ms-1'>Active</span></span>";
    }
    try {
        var d = new Date(utcYmdHm.replace(' ', 'T') + 'Z');
        var opts = { year: 'numeric', month: 'long', day: '2-digit' };
        if (tz && Intl && Intl.DateTimeFormat) opts.timeZone = tz;
        var ds = d.toLocaleDateString(locale, opts);
        var expiredJs = Date.now() > d.getTime();
        return expiredJs ? ("<span title='Expired on " + ds + "'>" + ds + " <span class='badge text-bg-danger ms-1'>Expired</span></span>") : ("<span title='Expires on " + ds + "'>" + ds + " <span class='badge text-bg-success ms-1'>Active</span></span>");
    } catch { return utcYmdHm; }
}
