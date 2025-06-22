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

function proxyCheckIpLink(ipAddress, riskScore, isProxy, isVpn, type = '') {
    let riskClass = 'text-bg-success';

    if (riskScore >= 80) {
        riskClass = 'text-bg-danger';
    } else if (riskScore >= 50) {
        riskClass = 'text-bg-warning';
    } else if (riskScore >= 25) {
        riskClass = 'text-bg-info';
    }

    let result = "<a href='https://proxycheck.io/v2/" + ipAddress + "' target='_blank'>" + ipAddress + "</a> ";
    result += "<span class='badge rounded-pill " + riskClass + "'>Risk: " + riskScore + "</span> ";

    if (type && type !== '') {
        result += "<span class='badge rounded-pill text-bg-primary'>" + type + "</span> ";
    }

    if (isProxy) {
        result += "<span class='badge rounded-pill text-bg-danger'>Proxy</span> ";
    }

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
