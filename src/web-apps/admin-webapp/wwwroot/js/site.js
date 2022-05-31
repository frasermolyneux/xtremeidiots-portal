﻿function escapeHtml(unsafe) {
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
    return gameTypeIcon(gameType) + " " + safeUsername;
}

function renderPlayerName(gameType, username, playerId) {
    var safeUsername = escapeHtml(username);
    return gameTypeIcon(gameType) + " <a href='/Players/Details/" + playerId + "'>" + safeUsername + "</a>";
}

function chatLogUrl(chatLogId) {
    return "<a href='/ServerAdmin/ChatLogPermaLink/" + chatLogId + "'>PermLink</a>";
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

    switch(gameType) {
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
    return "<a href='https://geo-location.net/Home/LookupAddress/" + ipAddress + "'>" + ipAddress + "</a>";
}

function manageClaimsLink(userId) {
    return '<div class="btn-group btn-group-sm" role="group">' +
        '<a type="button" class="btn btn-primary"  href="/User/ManageProfile/' +
        userId +
        '?filterGame=true"><i class="fa fa-key"></i> Manage Claims</a>' +
        "</div>";
}

function logOutUserLink(userId, antiForgeryToken) {
    return '<form class="form-inline" method="post" action="/User/LogUserOut" method="post">' +
        '<input id="id" name="id" type="hidden" value="' +
        userId +
        '\"/>' +
        '<button class="btn btn-primary" type="submit">Logout User</button>' +
        antiForgeryToken +
        '</form>';
}
