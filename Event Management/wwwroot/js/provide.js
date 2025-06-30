$(document).ready(function () {
    const container = $('#provideContainer');

    $.getJSON('/json/Provide.json', function (provides) {
        $.each(provides, function (index, provide) {
            const card = `
            <div class="card2">
                <img src="${provide.img}" alt="${provide.alt}" />
                <h3>${provide.title}</h3>
                 <p>${provide.description}</p>
            </div>`;
            container.append(card);
        });
    });
});