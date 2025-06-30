$(document).ready(function () {
    const container = $('#serviceContainer');

    $.getJSON('/json/Service.json', function (services) {
        $.each(services, function (index, service) {
            const card = `
                <div class="card">
                    <img src="${service.img}" alt="${service.alt}" />
                    <h3>${service.title}</h3>
                    <p>${service.description}</p>
                    <div class="readMore">Read More &gt;&gt;</div>
                </div>`;
            container.append(card);
        });
    });
});
