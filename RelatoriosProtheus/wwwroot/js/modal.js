$(document).ready(function () {
    var modal = document.getElementById('Modal')
    modal?.addEventListener('show.bs.modal', function (event) {
        var size = $(event.relatedTarget).data('bs-size');
        if (size != null) {
            $('.modal-dialog').addClass("modal-" + size);
        }
        $('.modal-title').text($(event.relatedTarget).text());
        Load(".modal-load", event.relatedTarget.getAttribute("formaction"));

    });

    var modalAlert = document.querySelector('.ModalAlert');
    modalAlert?.addEventListener('show.bs.modal', function (event) {
        var size = $(event.relatedTarget).data('bs-size');
        if (size != null) {
            $('.modal-dialog').addClass("modal-" + size);
        }
        $('.modal-title').text($(event.relatedTarget).text());
        Load(".modal-load", event.relatedTarget.getAttribute("formaction"));

    });
});

function Load(target, url) {
    $(target).load(url);
}