$(document).ready(function () {
    const container = $('#galleryContainer');

    $.getJSON('/json/Events.json', function (events) {
        $.each(events, function (index, event) {
            const card = `
            <div class="card3">
              <img src="${event.img}" alt="${event.alt}" />
              <div class="galCont">
                <p class="payee">${event.payee}</p>
                <h3>${event.title}</h3>
                <p>${event.description}</p>
                <div class="readMore">See more &gt;&gt;</div>
              </div>
            </div>`;
            container.append(card);
        });
    });
});
