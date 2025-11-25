// Función para descargar archivos
window.downloadFile = async function (filename, downloadName) {
    try {
        const response = await fetch(`/${filename}`);

        if (!response.ok) {
            console.error('Error al descargar archivo:', response.statusText);
            alert('No se pudo descargar el archivo. Por favor, intenta más tarde.');
            return;
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = downloadName || filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);

        console.log('✅ Archivo descargado correctamente');
    } catch (error) {
        console.error('Error en la descarga:', error);
        alert('Ocurrió un error al descargar el archivo.');
    }
};


// Función para toggle de visibilidad de contraseña
window.togglePasswordVisibility = function (inputId, buttonId) {
    const input = document.getElementById(inputId);
    const button = document.getElementById(buttonId);

    if (!input || !button) {
        console.error('No se encontró el input o el botón con los IDs proporcionados');
        return;
    }

    if (input.type === 'password') {
        input.type = 'text';
        button.innerHTML = '👁️‍🗨️'; // Ojo tachado (ocultar)
        button.setAttribute('title', 'Ocultar contraseña');
    } else {
        input.type = 'password';
        button.innerHTML = '👁️'; // Ojo abierto (mostrar)
        button.setAttribute('title', 'Mostrar contraseña');
    }
};
