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