/*(function () {
    $(function () {
        $('#input_apiKey').off();
        $('#input_apiKey').on('change', function () {
            var key = this.value;
            if (key && key.trim() !== '') {
                swaggerUi.api.clientAuthorizations.add("key", new SwaggerClient.ApiKeyAuthorization("LagKode", key, "header"));
            }
        });
    });
})();
*/
(function () {
    $(function () {
        console.log('***1here is my custom content!');
        var apiKeysUI =
            '<div class="input"><input placeholder="LagKode" id="input_LagKode" name="LagKode" type="text" size="10"/></div>' +
            '<div class="input"><input placeholder="DeltakerKode" id="input_DeltakerKode" name="DeltakerKode" type="DeltakerKode" size="10"/></div>';
        $(apiKeysUI).insertBefore('#api_selector div.input:last-child');
        $("#input_apiKey").hide();

        $('#input_LagKode').change(addAuthorization);
        $('#input_DeltakerKode').change(addAuthorization);
    });

    function addAuthorization() {
        var LagKode = $('#input_LagKode').val();
        var DeltakerKode = $('#input_DeltakerKode').val();
        if (LagKode && LagKode.trim() != "" && DeltakerKode && DeltakerKode.trim() != "") {
            /* 
            var basicAuth = new SwaggerClient.PasswordAuthorization('basic', LagKode, DeltakerKode);
            window.swaggerUi.api.clientAuthorizations.add("basicAuth", basicAuth);
            console.log("authorization added: LagKode = " + LagKode + ", DeltakerKode = " + DeltakerKode);
            */
            swaggerUi.api.clientAuthorizations.add("keyLagKode", new SwaggerClient.ApiKeyAuthorization("LagKode", LagKode, "header"));
            swaggerUi.api.clientAuthorizations.add("keyDeltakerKode", new SwaggerClient.ApiKeyAuthorization("DeltakerKode", DeltakerKode, "header"));
            console.log("clientAuthorizations added: LagKode = " + LagKode + ", DeltakerKode = " + DeltakerKode);
        }
    }
})();