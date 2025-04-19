document.getElementById("downloadBtn").addEventListener("click", function () {
    const link = document.createElement("a");
    link.href = "certificate-example.jpg";
    link.download = "Certificate.jpg";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
});

document.getElementById("shareBtn").addEventListener("click", function () {
    const shareUrl = window.location.href;
    navigator.clipboard.writeText(shareUrl).then(() => {
        alert("Đã sao chép liên kết chứng chỉ!");
    }).catch(err => {
        console.error("Lỗi khi sao chép link", err);
    });
});