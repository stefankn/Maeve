window.maeve = {
    scrollToBottom: function() {
        setTimeout(function() {
            window.scrollTo(0, document.body.scrollHeight);
        }, 100);
    }
}