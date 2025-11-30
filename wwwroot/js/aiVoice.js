window.aiVoice = {
    recognition: null,
    dotNetRef: null,

    initialize: function (dotNetReference) {
        this.dotNetRef = dotNetReference;

        // Verificar soporte del navegador
        if (!('webkitSpeechRecognition' in window) && !('SpeechRecognition' in window)) {
            console.error("Web Speech API no soportada en este navegador.");
            return false;
        }

        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        this.recognition = new SpeechRecognition();

        // Configuración
        this.recognition.continuous = false; // Detenerse al terminar de hablar
        this.recognition.lang = 'es-DO'; // Español (puedes cambiar a es-ES o es-MX)
        this.recognition.interimResults = true; // Mostrar resultados parciales

        // Eventos
        this.recognition.onstart = () => {
            this.dotNetRef.invokeMethodAsync('OnSpeechStart');
        };

        this.recognition.onend = () => {
            this.dotNetRef.invokeMethodAsync('OnSpeechEnd');
        };

        this.recognition.onresult = (event) => {
            let finalTranscript = '';
            let interimTranscript = '';

            for (let i = event.resultIndex; i < event.results.length; ++i) {
                if (event.results[i].isFinal) {
                    finalTranscript += event.results[i][0].transcript;
                } else {
                    interimTranscript += event.results[i][0].transcript;
                }
            }

            // Enviar texto a Blazor
            if (finalTranscript || interimTranscript) {
                this.dotNetRef.invokeMethodAsync('OnSpeechResult', finalTranscript, interimTranscript);
            }
        };

        this.recognition.onerror = (event) => {
            console.error("Error de reconocimiento de voz:", event.error);
            this.dotNetRef.invokeMethodAsync('OnSpeechError', event.error);
        };

        return true;
    },

    startRecording: function () {
        if (this.recognition) {
            try {
                this.recognition.start();
            } catch (e) {
                console.warn("El reconocimiento ya estaba iniciado.");
            }
        }
    },

    stopRecording: function () {
        if (this.recognition) {
            this.recognition.stop();
        }
    }
};