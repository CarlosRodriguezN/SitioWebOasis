﻿$(document).ready(function () {
    $.jgrid.defaults.width = 'auto';
    $.jgrid.defaults.responsive = true;
    $.jgrid.defaults.styleUI = 'Bootstrap';

    var grdEvAcumulativa = $('#dtaJsonEvAcumulativa').val();
    var grid = $("#grdEvAcumulativa");
    var lastsel;
    var lstEvaluaciones = new Array();
    var parcialActivo = "bytNota" + $("#dtaParcialActivo").val();
    var selIRow = 1;
    var rowIds;
    var gn1 = ($('#dtaParcialActivo').val() == "1") ? true : false;
    var gn2 = ($('#dtaParcialActivo').val() == "2") ? true : false;
    var gn3 = ($('#dtaParcialActivo').val() == "3") ? true : false;

    //  Objeto con informacion de dtaEvAcumulada
    cargarDatosEvAcumulativa($('#dtaJsonEvAcumulativa').val());

    //  Gestion de notas 
    grid.jqGrid({
        url: 'Docentes/',
        datatype: "json",
        colModel: [ { name: 'No', index: 'No', label: 'No', align: 'center', width: '20', sortable: false },
                    { name: 'sintCodMatricula', key: true, hidden: true },
                    { name: 'NombreEstudiante', label: "Nombre estudiante", align: 'left', width: '200', sortable: false },
                    { name: 'bytNumMat', label: 'Matricula', align: 'center', width: '50', sortable: false },

                    { name: 'bytNota1', label: 'Nota uno (1)', align: 'center', width: '50', editable: gn1, edittype: 'text', editoptions: { size: 1, maxlength: 2, dataInit: soloNumero }, editrules: { custom: true, custom_func: validarNota }, sortable: false, formatter: { integer: { thousandsSeparator: " ", defaultValue: '0' } } },
                    { name: 'bytNota2', label: 'Nota dos (2)', align: 'center', width: '50', editable: gn2, edittype: 'text', editoptions: { size: 1, maxlength: 2, dataInit: soloNumero }, editrules: { custom: true, custom_func: validarNota }, sortable: false, formatter: { integer: { thousandsSeparator: " ", defaultValue: '0' } } },
                    { name: 'bytNota3', label: 'Nota tres (3)', align: 'center', width: '50', editable: gn3, edittype: 'text', editoptions: { size: 2, maxlength: 2, dataInit: soloNumero }, editrules: { custom: true, custom_func: validarNota }, sortable: false, formatter: { integer: { thousandsSeparator: " ", defaultValue: '0' } } },

                    { name: 'Total', label: 'Total', align: 'center', width: '50', sortable: false },
                    { name: 'bytAsistencia', label: 'Asistencia (%)', align: 'center', width: '50', editable: true, edittype: 'text', editoptions: { size: 1, maxlength: 2, dataInit: soloNumero }, editrules: { custom: true, custom_func: validarAsistencia }, sortable: false, formatter: { integer: { thousandsSeparator: " ", defaultValue: '0' } } },
                    { name: 'ucAcumulado', label: 'Estado', width: '70', align: 'center' },
                    { name: 'strObservaciones', label: 'Observación', align: 'left', width: '100', sortable: false }],
        datatype: "jsonstring",
        datastr: $("#dtaJsonEvAcumulativa").val(),
        viewrecords: true,
        autowidth: true,
        height:"auto",
        shrinkToFit: true,
        ignoreCase: true,
        editurl: "Docentes",
        onSelectRow: function (id, status, e) {
            if (id !== lastsel) {
                //  Cierro edicion de la ultima fila gestionada
                if (lastsel != undefined) {
                    $('#grdEvAcumulativa').jqGrid('restoreRow', lastsel);
                }

                //
                //  Recalculo Acumulado - si el docente edito la nota recalcula el acumulado total, 
                //  cumplimiento y si esta en el ultimo parcial la equivalencia
                //  
                $("#grdEvAcumulativa").jqGrid("editRow", id, {
                    keys: true,
                    focusField: 4,
                    aftersavefunc: function (id) {
                        //  Registro la informacion gestionada en el JSON
                        guardarDtaEvaluacion(id);

                        //  Actualizo contenido de la fila
                        updDtaEvaluacion(id);

                        //  Obtengo el identificador de la siguiente registro de notas a gestionar
                        var idNextRow = getIdNextRow(id);
                        if (id != idNextRow) {
                            $('#grdEvAcumulativa').jqGrid('setSelection', idNextRow, true);
                        }
                    }
                });

                lastsel = id;
            }
        },

        loadComplete: function (data) {
            //  Resaltar contenido en columnas en la pagina
            updContenidoColumnasGrid();
        }
    });


    function validarNota(value, colname)
    {
        if (value < 0 || value > 10)
            return [false, "Nota fuera de rango (0, 10)"];
        else
            return [true, ""];
    }


    function validarAsistencia(value, colname)
    {
        if (value < 0 || value > 100) {
            return [false, "Porciento de asistencia fuera de rango (0, 100)"];
        } else {
            return [true, ""];
        }
    }


    function getIdNextRow(idActualRow) {
        var num = rowIds.length;
        var idNextRow = idActualRow;

        for (var x = 0; x < num; x++) {
            if (rowIds[x] == idActualRow && x < num) {
                idNextRow = rowIds[x + 1];
                break;
            }
        }

        return idNextRow;
    }


    function cargarDatosEvAcumulativa(dtaEvAcumulada)
    {
        var dtaEvaluaciones = eval(dtaEvAcumulada);
        if (dtaEvaluaciones.length > 0) {
            var nee = dtaEvaluaciones.length;
            for (var x = 0; x < nee; x++) {
                var objEvaluaciones = new EvaluacionAcumulativa();
                objEvaluaciones.setDtaEvaluacion(dtaEvaluaciones[x], $("#dtaParcialActivo").val());
                lstEvaluaciones.push(objEvaluaciones);
            }
        }
    }


    function updDtaEvaluacion(id)
    {
        numReg = lstEvaluaciones.length;
        for (var x = 0; x < numReg; x++) {
            if (lstEvaluaciones[x].sintCodMatricula == id) {
                $("#grdEvAcumulativa").jqGrid('setRowData', id, { Total: lstEvaluaciones[x].acumulado() });
                $("#grdEvAcumulativa").jqGrid('setRowData', id, { ucAcumulado: lstEvaluaciones[x].getEstadoEvaluacion() });

                break;
            }
        }

        $("#grdEvAcumulativa").jqGrid(  'setCell',
                                        id,
                                        parcialActivo,
                                        "",
                                        {   'background-color': '#dff0d8',
                                            'background-image': 'none',
                                            'text-align': 'center',
                                            'font-size': 'medium',
                                            'font-weight': 'bold' });

        return true;
    }


    function updContenidoColumnasGrid()
    {
        rowIds = $('#grdEvAcumulativa').jqGrid('getDataIDs');

        for (i = 0; i <= rowIds.length; i++) {
            //  En funcion al parcial activo resalto el color de la columna 
            $("#grdEvAcumulativa").jqGrid(  'setCell',
                                            rowIds[i],
                                            parcialActivo,
                                            "",
                                            { 'background-color': '#fcf8e3' });

            $("#grdEvAcumulativa").jqGrid('setRowData', rowIds[i], { bytNumMat: lstEvaluaciones[i].getNumMatricula() });

            //  Si el parcial activo es el 3ro actualizo el cumplimiento
            $("#grdEvAcumulativa").jqGrid('setRowData', rowIds[i], { ucAcumulado: lstEvaluaciones[i].getEstadoEvaluacion() });
        }
    }


    $("#grdEvAcumulativa").jqGrid('setGroupHeaders', {
        useColSpanStyle: true,
        groupHeaders: [ { startColumnName: 'bytNota1', numberOfColumns: 3, titleText: '<b>EVALUACIÓN ACUMULATIVA</b>' },
                        { startColumnName: 'Total', numberOfColumns: 3, titleText: '<b>TOTALIZADOS</b>' }]
    });


    function guardarDtaEvaluacion(id)
    {
        numReg = lstEvaluaciones.length;
        for (var x = 0; x < numReg; x++) {
            if (lstEvaluaciones[x].sintCodMatricula == id) {
                var dtaNota = $("#grdEvAcumulativa").jqGrid("getCell", id, "bytNota" + $('#dtaParcialActivo').val());
                var dtaAsistencia = $("#grdEvAcumulativa").jqGrid("getCell", id, "bytAsistencia");

                lstEvaluaciones[x]["bytNota" + $('#dtaParcialActivo').val()] = dtaNota;
                lstEvaluaciones[x].bytAsistencia = dtaAsistencia;
                lstEvaluaciones[x].banEstado = 1;

                return true;
            }
        }

        return false;
    }


    function soloNumero(element)
    {
        $(element).keypress(function (e) {
            if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
                return false;
            }
        });
    }


    $('#btnGuardarEvAcumulativa').on('click', function(){

        $.ajax({type: "POST",
                url: "/Docentes/registrarEvaluacion/" + $("#nivel").val() + "/" + $("#codAsignatura").val() + "/" +  $("#paralelo").val() + "/" + $('#dtaParcialActivo').val(),
                data: JSON.stringify(lstEvaluaciones),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                error: function (xhr, ajaxOptions, thrownError) {
                    alert(thrownError);
                }
        }).success(function (data) {
            $('#msmGrdEvAcumulativa').removeAttr("hidden");
            cargarDatosEvAcumulativa(data);
            updContenidoColumnasGrid();
        })

    })


    jQuery("tr.ui-jqgrid-labels th:eq(1)").attr("title", "Column 1 header title")

})