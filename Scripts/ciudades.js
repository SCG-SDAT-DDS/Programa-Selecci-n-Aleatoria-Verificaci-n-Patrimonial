(function () {
    //Primera Carga
    const pais = document.getElementById('PaisId');
    const estado = document.getElementById('EstadoId');  //$('#EstadoId');
    const ciudad = document.getElementById('CiudadId');
    let paisFirstValue = pais != null ? pais.item(pais.selectedIndex) != null ? pais.item(pais.selectedIndex).value : '' : '';
    let estadoSelected = estado != null ? estado.item(estado.selectedIndex) != null ? estado.item(estado.selectedIndex).value : '' : '';


    if (paisFirstValue != null && paisFirstValue != '') {
        cargarEstados(paisFirstValue);
    }
    else {
        cargarPEC();
    }

    //Cargar todos los Paises, estados y ciudades 
    function CargarPEC() {
        if (pais != null) {
            cargarTodosLosPaises();
        }

        if (estado != null) {
            cargarTodosLosEstados();
        }

        if (ciudad != null) {
            cargarTodasLasCiudades();
        }
    }

    function cargarTodasLasCiudades() {
        $.ajax({
            type: 'POST',
            url: '/Ciudades/ObtenerTodasLasCiudades',
            success: function (data) {
                agregarOpcionesSelect(estado, data, true);
            }
        });
    }

    function cargarTodosLosEstados() {

        $.ajax({
            type: 'POST',
            url: '/Estados/ObtenerTodosLosEstados',
            success: function (data) {
                agregarOpcionesSelect(estado, data, true);
            }
        });
    }

    function cargarTodosLosPaises() {
        $.ajax({
            type: 'POST',
            url: '/Paises/ObtenerTodosLosPaises',
            success: function (data) {
                agregarOpcionesSelect(estado, data, true);
            }
        });
    }

    function cargarEstados(paisId) {
        $.ajax({
            type: 'POST',
            url: '/Estados/ObtenerEstadosDePais',
            data: {
                id: paisId
            },
            success: function (data) {
               
                agregarOpcionesSelect(estado, data, true);
                let estadoId = estado != null ? estado.value : '';

                cargarCiudades(estadoId);
            }
        });
    }

    function cargarCiudades(estadoId) {
        if (estadoId == '') {
            cargarTodasLasCiudades();
        } else {
            $.ajax({
                type: 'POST',
                url: '/Ciudades/ObtenerCiudadesDeEstado',
                data: {
                    id: estadoId
                },
                success: function (data) {
                    agregarOpcionesSelect(ciudad, data, true);
                }
            });
        }
    }

    function agregarOpcionesSelect(select, lista, clean) {
        if (clean) {
            limpiarOptionSelect(select);
        }
        for (var i = 0; i < lista.length; i++) {
            let opt = document.createElement('option');
            opt.value = lista[i].Value;
            opt.innerHTML = lista[i].Text;
            select.appendChild(opt);
        }
    }

    function limpiarOptionSelect(select) {
        if (select != null) {
            for (var i = select.length; i >= 0; i--) {
                select.remove(i);
            }
        }
    }

    //Eventos change de combos
    if (pais != null) {
        pais.addEventListener('change', (event) => {
            cargarEstados(event.target.value);
        });
    }
    
    if (estado != null) {
        estado.addEventListener('change', (event) => {
            estadoSelected = event.target.value;
            cargarCiudades(estadoSelected);
        });
    }

})();