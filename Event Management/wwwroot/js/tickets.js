$(document).ready(function () {
    function togglePaidFields() {
        const selected = $('input[name="IsPaid"]:checked').val();
        if (selected === 'Paid') {
            $('#ticketPricingGroup').show();
            $('#paymentMethodsGroup').show();
        } else {
            $('#ticketPricingGroup').hide();
            $('#paymentMethodsGroup').hide();
        }
    }

    // Run on page load
    togglePaidFields();

    // Run on change
    $('input[name="IsPaid"]').on('change', togglePaidFields);
});
