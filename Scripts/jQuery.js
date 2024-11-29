$.ajaxSetup({
    beforeSend: function () {
        ShowLoading();
    },
    complete: function () {
        HideLoading();
        $("#ibox-index").children('.ibox-content').removeClass('sk-loading');
    }
});

//$("form").submit(function () {
//    ShowLoading();
//});

//$("a").click(function () {
//    ShowLoading();
//});
//$('a').click(function (e) {
//    ShowLoading();
//    //e.preventDefault();
//    var destination = this.href;
//    console.log(destination);
//    if (destination.length <= 0) {
//        HideLoading();
//    }
//});

//function ValidarArchivoExcel(vElemento) {
//    if ($(vElemento).val().length > 0) {
//        var vExtensionsAllowed = ["xls", "xlsx"];
//        var vSplitFile = $(vElemento).val().split(".");
//        var vExtensionFile = vSplitFile[vSplitFile.length - 1].toLowerCase();

//        if (vExtensionsAllowed.indexOf(vExtensionFile) == -1) {
//            MyToast("Aviso", "Archivo no permitido, debe de ser excel.", "warning");
//            $(vElemento).val("");
//        }
//    }
//}


$(function () {

    //Mostrar limpiar filtro cuando este lleno
    //$(".clean-filter").each(function (index, item) {
    //    if ($(item).prev().val().length > 0) {
    //        $(this).css({ display: "table-cell" });
    //    }
    //})

    //$('.selectpicker').selectpicker({ liveSearchNormalize: true, liveSearch: true, header: "Seleccionar", noneResultsText: "No se encontraron resultados con {0}" });

    //var myElem = document.getElementById('pagedList');
    //if (myElem != null) {
    //    var str = document.getElementById("pagedList").innerHTML;
    //    var res = str.replace("Showing items", "Elementos del");
    //    res = res.replace("through", "al");
    //    res = res.replace("of", "de");
    //    document.getElementById("pagedList").innerHTML = res;
    //}


    //$("#pagedList .pagination-container .pagination").addClass("pagination-sm");


    
    //CreateAutocompletesBootstrap();//CreateAutocompletes();


    //var getPage = function () {
    //    var $a = $(this)

    //    var options = {
    //        url: $a.attr("href"),
    //        data: $("form").serialize(),
    //        type: "get"
    //    };

    //    $.ajax(options).done(function (data) {
    //        var target = $a.parents("div.pagedList").attr("data-otf-target");
    //        $(target).replaceWith(data);
    //    });
    //    return false;
    //};


    //$("body").on("click", ".pagedList a", getPage);
 //   $(".pagedList a").click(getPage);

 //   $('.datepicker').datepicker({
 //       format: "mm/dd/yyyy",
 //       todayBtn: "linked",
 //       language: "es",
 //       autoclose: true,
 //       todayHighlight: true
 //   });

 //$('.datepicker-uni').datepicker({
 //    format: "dd/mm/yyyy",
 //       todayBtn: "linked",
 //       language: "es",
 //       autoclose: true,
 //       todayHighlight: true
 //   });

    //$('.datepicker-year').datepicker({
    //    format: "yyyy",
    //    todayBtn: "linked",
    //    language: "es",
    //    autoclose: true,
    //    todayHighlight: true
    //});
    
    //$(".datepicker-year").datepicker({
    //    format: "yyyy",
    //    viewMode: "years",
    //    minViewMode: "years",
    //    todayBtn: "linked",
    //    language: "es",
    //    autoclose: true,
    //    todayHighlight: true
    //});

    //$("#btnCleanFilter").click(function (event) {
    //    event.preventDefault();
    //    $("input").val("");
    //    //$("select").val("");
    //    $("select").prop("selectedIndex", 0);
    //    $("#sOrder").val("");
    //    $('select.select2').select2("val", 0);
    //    $("#ibox-index").children('.ibox-content').addClass('sk-loading');
    //    $(".clean-filter").css({ display: "none" });
    //    $("#iFiltros").val(1);
    //    $("#form0").submit();
    //});

  


    //$(".change-checked").click(function()
    //{
    //    var vElementId = $(this).attr("element-id");
    //    if($(this).hasClass("fa-square-o"))
    //    {
    //        $(this).removeClass("fa-square-o").addClass("fa-check-square-o");
    //        $("#" + vElementId).val(true);
    //    }
    //    else
    //    {
    //        $(this).removeClass("fa-check-square-o").addClass("fa-square-o");
    //        $("#" + vElementId).val(false);
    //    }
    //})

    //$(".filtrar")
    //    .change(function ()
    //    {
    //        var vElement = $(this);
    //        if ($(vElement).val().length > 0) {
    //            $(vElement).siblings(".clean-filter").css({ display: "table-cell" });
    //        }
    //        else {
    //            $(vElement).siblings(".clean-filter").css({ display: "none" });
    //        }
    //        $("#ibox-index").children('.ibox-content').addClass('sk-loading');
    //        $(vElement).blur();
    //        setTimeout(function () {
    //            $("#form0").submit();
    //        }, 200);
    //    })
    //    .keypress(function()
    //    {
    //        if(event.charCode == 13)
    //        {
    //            $("#ibox-index").children('.ibox-content').addClass('sk-loading');
    //            $("#form0").submit();
    //        }
    //    })
    //    .keyup(function()
    //    {
    //        if ($(this).val().length > 0)
    //        {
    //            $(this).siblings(".clean-filter").css({ display: "table-cell" });
    //        }
    //        else
    //        {
    //            $(this).siblings(".clean-filter").css({ display: "none" });
    //        }
    //    })

    //$('.phone-mask').mask('000-000-0000');

    //$("#pagedList a[href]").click(function()
    //{
    //   $("#ibox-index").children('.ibox-content').addClass('sk-loading');
    //    $("#ibox-bitacora").children('.ibox-content').addClass('sk-loading');
    //})

    //$(".select2").select2({
    //    width: "100%",
    //    language: {
    //        noResults: function () {
    //            return "No se encontraron resultados.";
    //        }
    //    }
    //});

    //$(".clean-filter").click(function()
    //{
    //    var $vElement = $(this).parent().find(".filtrar");
    //    $vElement.val("").trigger("change");
    //    if ($vElement.hasClass("select2"))
    //    {
    //        $vElement.select2("val", 0);
    //    }
    //})

});

//function CreateAutocompletesBootstrap()
//{
//    $('[data-autocomple]').each(function ()
//    {
//        //var vUrl = $(this).attr("data-autocomple") + "?term=" + $(this).val();
//        //$(this).typeahead({
//        //    ajax: {
//        //        url: vUrl
//        //    }
//        //});

       
//        var myAccentMap = { "á": "a", "é": "e", "í": "i", "ó": "o", "ú": "u" };
//        var vUrl = $(this).attr("data-autocomple");// + "?term=" + $(this).val();
//        var vElement = $(this);
//        $.get(vUrl, function (data) {
//            $(vElement).typeahead({
//                source: data,
//                autoSelect: false,
//                limit: 10
//                //updater: function(data)
//                //{
//                //    return data;
//                //}
//            });
//        }, 'json');
//    })
//}

//function CreateAutocompletes()
//{
//    var createAutocomplete = function () {
//        var $input = $(this);
//        var options = {
//            source: $input.attr("data-autocomple")
//        };
//        $input.autocomplete(options);
//    };

//    $("input[data-autocomple]").each(createAutocomplete)
//}

//function OrderBy(vElement)
//{
//   $("#ibox-index").children('.ibox-content').addClass('sk-loading');
//    var vOrder = $(vElement).attr("order");
//    $(vElement).find("i").toggleClass("");
//    $("#sOrder").val(vOrder);
//    $("#form0").submit();
//}


$(document).ajaxSuccess(function()
{
    //$("#pagedList .pagination-container .pagination").addClass("pagination-sm");
    //CreateAutocompletesBootstrap();//CreateAutocompletes();

    //$(".filtrar").change(function () {
    //    $("#ibox-index").children('.ibox-content').addClass('sk-loading');
    //    $("#form0").submit();
    //})

    //$("#btnCleanFilter").click(function (event) {
    //    event.preventDefault();
    //    $("input").val("");
    //    $("select").val("");
    //    $("#sOrder").val("");
    //    $("#ibox-index").children('.ibox-content').addClass('sk-loading');
    //    $("#form0").submit();
    //})


    $("#pagedList a[href]").click(function () {
        $("#ibox-index").children('.ibox-content').addClass('sk-loading');
        $("#ibox-bitacora").children('.ibox-content').addClass('sk-loading');
    })
})


//function MyToast(vTitulo, vMensaje, vTipo, vTiempo)
//{
//    vTiempo = vTiempo == undefined ? "5000" : vTiempo;
//    toastr.options = {
//        "closeButton": true,
//        "debug": false,
//        "progressBar": true,
//        "preventDuplicates": true,
//        "positionClass": "toast-top-center",//toast-top-right
//        "onclick": null,
//        "showDuration": "400",
//        "hideDuration": "1000",
//        "timeOut": vTiempo,
//        "extendedTimeOut": "1000",
//        "showEasing": "swing",
//        "hideEasing": "linear",
//        "showMethod": "slideDown",
//        "hideMethod": "slideUp"
//    }

//    toastr[vTipo](vMensaje, vTitulo);
//}

var ShowLoading = function () {
    $(".cover-spin2").show();
    $(".cover-spin2").addClass('in');
}

var HideLoading = function () {
    setTimeout(function () {
        $(".cover-spin2").removeClass('in')
        setTimeout(function () {
            $(".cover-spin2").hide();
        }, 200);
    }, 400)
}