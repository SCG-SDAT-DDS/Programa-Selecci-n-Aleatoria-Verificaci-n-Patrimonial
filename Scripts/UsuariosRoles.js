const usuarioId = $('#Id').val();
const divMensajeModal = $('#modalMessage');
const divMensajeRol = $('#mensajeRol');

divMensajeModal.hide();

$('#btnAsignar').click(function () {
    limpiaDivMensajeModal();
    let nombreRol = $('#Rol').val();
    var url = $(this).data('asigna-url');
    let vOrganismoID = $('#OrganismoID').val();
    let vUnidadAdministrativaId = $("#UnidadAdministrativaId").val();

    $.ajax({
        type: 'POST',
        url: url,
        data: {
            id: usuarioId,
            nombreRol: nombreRol,
            OrganismoID: vOrganismoID,
            UnidadAdministrativaId: vUnidadAdministrativaId
        },
        success: function (data) {
            ponerMensajeModal(data);
        }
    });
});

$('#confirm-delete').on('show.bs.modal', function (e) {
    let rol = $(e.relatedTarget).data('rol-name');
    let url = $(e.relatedTarget).data('eliminarol-url');
    $('.rolModal-confirm').html('<strong>' + rol + '</strong>');
    $(this).find('.btn-ok').attr('data-rol-name', rol).attr('data-url', url);
});

$('#btnEliminaRol').click(function () {
    let nombreRol = document.querySelector('.btn-ok').getAttribute('data-rol-name');
    var url = $(this).data('url');
    RemoverRol(nombreRol, url)
        .then(function(data) {
            ponerMensajeRol(data);
            $("#btnCancelRemove").click();

        })
        .catch(onErrorRol);

    //limpiaDivMensajeRol();

});

function RemoverRol (rolName, url) {
    return new Promise((resolve, reject)  => {
        
        $.get(url, { usuarioId: usuarioId, nombreRol: rolName }, function (data) {
            resolve(data);
        })
        .fail(() => reject(rolName))
    });
}

function onErrorRol(rolName) {
  console.log(`Sucedió un error al remover el rol ${rolName}`)
}

$('#rolesModal').on('show.bs.modal', function (e) {
    limpiaDivMensajeModal();
    divMensajeModal.html('');
});

function limpiaDivMensajeModal() {
    if (divMensajeModal.hasClass('alert')) {
        divMensajeModal.removeClass('alert');
        
    }
    if (divMensajeModal.hasClass('alert-success')) {
        divMensajeModal.removeClass('alert alert-success');
    }
    if (divMensajeModal.hasClass('alert-warning')) {
        divMensajeModal.removeClass('alert alert-warning');
    }
    if (divMensajeModal.hasClass('alert-danger')) {
        divMensajeModal.removeClass('alert alert-danger');
    }
}

function limpiaDivMensajeRol() {
    divMensajeRol.html('');
    if (divMensajeRol.hasClass('alert-success')) {
        divMensajeRol.removeClass('alert alert-success');
    }
    if (divMensajeRol.hasClass('alert-warning')) {
        divMensajeRol.removeClass('alert alert-warning');
    }
    if (divMensajeRol.hasClass('alert-danger')) {
        divMensajeRol.removeClass('alert alert-danger');
    }
}

function ponerMensajeModal(data) {
    
    limpiaDivMensajeModal();
    if (data.status == "ok") {
        divMensajeModal.addClass('alert alert-success')
        divMensajeModal.html('Rol asignado existosamente.');
        divMensajeModal.show();
        actualizaRoles();
    }
    if (data.status == "fallo") {
        divMensajeModal.addClass('alert alert-danger')
        divMensajeModal.html('Ocurrió un problema al asignar el Rol.');
        divMensajeModal.show();
    } 

    if (data.status == "falloUnidadAdministrativa") {
        divMensajeModal.addClass('alert alert-danger')
        divMensajeModal.html('Es necesario seleccionar una Unidad Administrativa para guardar.');
        divMensajeModal.show();
    }
    if (data.status == "falloOrganismo") {
        divMensajeModal.addClass('alert alert-danger')
        divMensajeModal.html('Es necesario seleccionar un Organismo para guardar.');
        divMensajeModal.show();
    }
    if (data.status == "existe") {
        divMensajeModal.addClass('alert alert-warning')
        divMensajeModal.html('El rol ya se encuentra asignado al usuario.');
        divMensajeModal.show();
    }
    if (data.status == "noexisterol") {
        divMensajeModal.addClass('alert alert-danger')
        divMensajeModal.html('El rol no existe o no esta asociado al usuario.');
        divMensajeModal.show();
    }
}

function ponerMensajeRol(data) {
    limpiaDivMensajeRol();
    //console.log("ok");
    //console.log(data);
    if (data.status == "ok") {
        divMensajeRol.addClass('alert alert-success')
        divMensajeRol.html('La asignación se elimino existosamente.');
        divMensajeRol.show();
        actualizaRoles();
        console.log("Ok");
    }
    if (data.status == "fallo") {
        divMensajeRol.addClass('alert alert-danger')
        divMensajeRol.html('Ocurrió un problema al eliminar el Rol, si es el unico rol que cuenta el usuario no podra eliminar este.');
        divMensajeRol.show();
        console.log("fallo");
    }
    if (data.status == "noexiste") {
        divMensajeRol.addClass('alert alert-warning')
        divMensajeRol.html('La Asignación no existe o no esta asociado al usuario.');
        divMensajeRol.show();
        console.log("noexiste");
    }
    if (data.status == "admin") {
        divMensajeRol.addClass('alert alert-danger')
        divMensajeRol.html('No se puede eliminar el rol Administrador al usuario Administrador.');
        divMensajeRol.show();
        console.log("admin");
    }
}

function actualizaRoles() {
    $.ajax({
        type: 'POST',
        url: '/UsuariosAdmin/ActualizaListaRoles',
        data: {
            id: usuarioId
        },
        success: function (data) {
            $('#divRolesUsuario').html(data);
        }
    });
}