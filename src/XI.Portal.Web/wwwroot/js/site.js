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

function renderPlayerName (gameType, username) {
    var safeUsername = escapeHtml(username);

    return "<img src='/images/game-icons/" +
        gameType +
        ".png' alt='" +
        gameType +
        "' width='16' height='16' /> " + safeUsername;
}

function chatLogUrl(chatLogId) {
    return "<a href='/ServerAdmin/ChatLogPermaLink/" + chatLogId + "'>PermLink</a>";
}
