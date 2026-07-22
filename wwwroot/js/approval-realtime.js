// Realtime cho luồng duyệt "phí khác". Dùng SignalR hub /hubs/approval.
(function () {
    if (typeof signalR === "undefined") {
        return; // thư viện signalr chưa tải
    }

    var pendingFeeCreatedCallbacks = [];

    function updateBadges(count) {
        document.querySelectorAll(".js-pending-badge").forEach(function (el) {
            el.textContent = count;
            el.style.display = count > 0 ? "" : "none";
        });
        var pageCount = document.getElementById("pending-count");
        if (pageCount) {
            pageCount.textContent = count;
        }
    }

    function toast(message) {
        var box = document.createElement("div");
        box.className = "alert alert-info shadow position-fixed";
        box.style.cssText = "top:1rem;right:1rem;z-index:1080;max-width:360px;";
        box.textContent = message;
        document.body.appendChild(box);
        setTimeout(function () { box.remove(); }, 6000);
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/approval")
        .withAutomaticReconnect()
        .build();

    connection.on("PendingFeeCreated", function (payload) {
        if (payload && typeof payload.count === "number") {
            updateBadges(payload.count);
        }
        toast((payload && payload.content) || "Có khoản phí mới chờ duyệt.");
        pendingFeeCreatedCallbacks.forEach(function (cb) {
            try { cb(payload); } catch (e) { /* noop */ }
        });
    });

    connection.on("PendingCountChanged", function (payload) {
        if (payload && typeof payload.count === "number") {
            updateBadges(payload.count);
        }
    });

    connection.on("FeeReviewed", function (payload) {
        toast((payload && payload.content) || "Khoản phí của bạn đã được xử lý.");
    });

    connection.start().catch(function () { /* offline: bỏ qua */ });

    // Lấy số phí chờ duyệt ban đầu (cho badge trên nav).
    fetch("/Tuition/PendingCount", { headers: { "X-Requested-With": "XMLHttpRequest" } })
        .then(function (r) { return r.ok ? r.json() : null; })
        .then(function (data) { if (data) { updateBadges(data.count); } })
        .catch(function () { /* noop */ });

    window.fapApproval = {
        onPendingFeeCreated: function (cb) { pendingFeeCreatedCallbacks.push(cb); }
    };
})();
