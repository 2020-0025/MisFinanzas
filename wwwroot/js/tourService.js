// tourService.js - Servicio para gestionar tours con Driver.js

window.tourService = {
    driver: null,

    init: function () {
        if (!window.driver || !window.driver.js || typeof window.driver.js.driver !== 'function') {
            console.error('Driver.js no está cargado correctamente');
            console.log('window.driver:', window.driver);
            return;
        }
        this.driver = window.driver.js.driver({
            showProgress: true,
            showButtons: ['next', 'previous', 'close'],
            nextBtnText: 'Siguiente →',
            prevBtnText: '← Anterior',
            doneBtnText: '✓ Finalizar',
            closeBtnText: '✕',
            progressText: '{{current}} de {{total}}',
            popoverClass: 'driverjs-theme',
            // ===== CONFIGURACIÓN DEL OVERLAY =====
            overlayColor: '#000',           // Color base del overlay
            overlayOpacity: 0.35,           // Opacidad del overlay (35%)
            smoothScroll: true,             // Scroll suave al elemento
            allowClose: true,               // Permitir cerrar con ESC o click
            doneBtnText: '✓ Finalizar',
            // ===== CONFIGURACIÓN DE ANIMACIÓN =====
            animate: true,                  // Habilitar animaciones
            // ===== PADDING DEL ELEMENTO DESTACADO =====
            stagePadding: 5,                // Padding alrededor del elemento destacado
            stageRadius: 8                  // Radio de borde del elemento destacado
        });
    },

    // Tour del Dashboard
    startDashboardTour: function () {
        this.init();

        // Definir todos los pasos posibles
        const allSteps = [
            {
                popover: {
                    title: '📊 Bienvenido(a)',
                    description: 'Aquí puedes ver un resumen completo de tu situación financiera actual. Te guiaré por las secciones principales.',
                }
            },
            {
                element: '#dashboard-summary-cards',
                popover: {
                    title: '💰 Resumen Financiero',
                    description: 'Visualiza tu balance general, ingresos totales, gastos, progreso de metas, y prestamos en estas cinco tarjetas.',
                    side: 'bottom',
                    align: 'start'
                }
            },
            {
                element: '#dashboard-income-expense-chart',
                popover: {
                    title: '📊 Gráfica de Ingresos vs Gastos',
                    description: 'Compara visualmente tus ingresos y gastos en el período seleccionado.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#dashboard-period-selector',
                popover: {
                    title: '📅 Cambiar Período',
                    description: 'Alterna entre ver datos de los últimos 7 días o 30 días.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#dashboard-expenses-by-category',
                popover: {
                    title: '🧾 Gastos por Categorías',
                    description: 'Visualiza cómo se distribuyen tus gastos entre diferentes categorías.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#dashboard-budget-chart',
                popover: {
                    title: '💰 % de Presupuesto Gastado',
                    description: 'Monitorea qué porcentaje de cada presupuesto has utilizado para evitar sobregastos.',
                    side: 'right',
                    align: 'center'
                }
            },
            {
                element: '#dashboard-budget-alerts',
                popover: {
                    title: '⚠️ Alertas de Presupuestos',
                    description: 'Recibe notificaciones cuando te acerques o excedas tus límites de presupuesto.',
                    side: 'left',
                    align: 'start'
                }
            },
            {
                element: '#dashboard-active-goals',
                popover: {
                    title: '🎯 Metas Activas',
                    description: 'Revisa el progreso de tus objetivos de ahorro y cuánto te falta para alcanzarlos.',
                    side: 'right',
                    align: 'start'
                }
            },
            {
                element: '#dashboard-recent-transactions',
                popover: {
                    title: '💳 Historial de Transacciones',
                    description: 'Consulta tus últimas transacciones con detalles de categoría, monto, descripción y fecha.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#dashboard-view-all-link',
                popover: {
                    title: '🔗 Ver Todas las Transacciones',
                    description: 'Haz clic aquí para acceder al historial completo de tus gastos e ingresos.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#dashboard-register-transaction',
                popover: {
                    title: '➕ Registrar Transacción',
                    description: 'Si no tienes transacciones, puedes empezar registrando tu primer gasto o ingreso aquí.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-help',
                popover: {
                    title: '❓ Boton de ayuda',
                    description: '¿Necesitas volver a ver el tutorial? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-home',
                popover: {
                    title: '🏠 Volver a Bienvenida',
                    description: '¿Necesitas ir a la pantalla de bienvenida? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-finn',
                popover: {
                    title: '🤖 Boton de Finn',
                    description: '¿Necesitas ayuda de Finn? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            }
        ];

        // Filtrar solo los pasos cuyos elementos existen en el DOM
        const availableSteps = allSteps.filter(step => {
            // El paso sin elemento (bienvenida) siempre se incluye
            if (!step.element) return true;

            // Solo incluir si el elemento existe en el DOM
            return document.querySelector(step.element) !== null;
        });

        this.driver.setSteps(availableSteps);
        this.driver.drive();
    },

    // Tour de Presupuestos
    startBudgetsTour: function () {
        this.init();

        // Definir todos los pasos posibles
        const allSteps = [
            {
                popover: {
                    title: '💰 Presupuestos',
                    description: 'Los presupuestos te ayudan a controlar cuánto gastas en cada categoría mensualmente. Te guiaré por las funciones principales.',
                }
            },
            {
                element: '#budgets-add-button',
                popover: {
                    title: '➕ Crear Presupuesto',
                    description: 'Haz clic aquí para crear un nuevo presupuesto y establecer límites de gasto para cualquier categoría.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#budgets-period-selector',
                popover: {
                    title: '📅 Selector de Período',
                    description: 'Cambia el mes y año para ver presupuestos de diferentes períodos, o vuelve rápidamente al mes actual.',
                    side: 'bottom',
                    align: 'start'
                }
            },
            {
                element: '#budgets-summary',
                popover: {
                    title: '📊 Resumen del Período',
                    description: 'Visualiza el presupuesto total asignado, cuánto has gastado y cuánto te queda disponible del mes seleccionado.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#budgets-list',
                popover: {
                    title: '📋 Presupuestos por Categoría',
                    description: 'Aquí verás cada presupuesto con su progreso, estado y alertas si te acercas al límite o lo excedes.',
                    side: 'top',
                    align: 'start'
                }
            },
            {
                element: '#budgets-edit-button',
                popover: {
                    title: '📝 Editar Presupuesto',
                    description: 'Usa este botón para modificar el nombre o monto asignado a un presupuesto existente.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#budgets-delete-button',
                popover: {
                    title: '🗑️ Eliminar Presupuesto',
                    description: 'Elimina presupuestos que ya no necesites. Esta acción no se puede deshacer.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#budgets-create-first',
                popover: {
                    title: '🚀 Crear tu Primer Presupuesto',
                    description: 'Si aún no tienes presupuestos, puedes crear el primero aquí para empezar a controlar tus gastos.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#budgets-copy-previous',
                popover: {
                    title: '📋 Copiar Mes Anterior',
                    description: 'Ahorra tiempo copiando los presupuestos del mes anterior en lugar de crearlos manualmente uno por uno.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#btn-help',
                popover: {
                    title: '❓ Boton de ayuda',
                    description: '¿Necesitas volver a ver el tutorial? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-home',
                popover: {
                    title: '🏠 Volver a Bienvenida',
                    description: '¿Necesitas ir a la pantalla de bienvenida? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-finn',
                popover: {
                    title: '🤖 Boton de Finn',
                    description: '¿Necesitas ayuda de Finn? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            }
        ];

        // Filtrar solo los pasos cuyos elementos existen en el DOM
        const availableSteps = allSteps.filter(step => {
            // El paso sin elemento (bienvenida) siempre se incluye
            if (!step.element) return true;

            // Solo incluir si el elemento existe en el DOM
            return document.querySelector(step.element) !== null;
        });

        this.driver.setSteps(availableSteps);
        this.driver.drive();
    },

    // Tour de Categorías
    startCategoriesTour: function () {
        this.init();

        // Definir todos los pasos posibles
        const allSteps = [
            {
                popover: {
                    title: '🏷️ Categorías',
                    description: 'Las categorías te permiten organizar tus ingresos y gastos de forma clara y personalizada. Te guiaré por las funciones principales.',
                }
            },
            {
                element: '#categories-add-button',
                popover: {
                    title: '➕ Crear Categoría',
                    description: 'Haz clic aquí para crear nuevas categorías personalizadas con nombres e iconos únicos. Tambien puedes asignarles presupuestos y recordatorios según desees.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#categories-expenses-section',
                popover: {
                    title: '📉 Categorías de Gastos',
                    description: 'Aquí se muestran todas tus categorías de gastos. Puedes editarlas o eliminarlas según tus necesidades.',
                    side: 'top',
                    align: 'start'
                }
            },
            {
                element: '#categories-expense-edit-button',
                popover: {
                    title: '📝 Editar Categoría de Gasto',
                    description: 'Usa este botón para modificar el nombre, icono o configuración de una categoría de gasto.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#categories-expense-delete-button',
                popover: {
                    title: '🗑️ Eliminar Categoría de Gasto',
                    description: 'Elimina categorías que ya no uses. Ten cuidado: esta acción puede afectar registros existentes.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#categories-incomes-section',
                popover: {
                    title: '📈 Categorías de Ingresos',
                    description: 'Aquí se muestran todas tus categorías de ingresos para organizar tus fuentes de dinero.',
                    side: 'top',
                    align: 'start'
                }
            },
            {
                element: '#categories-income-edit-button',
                popover: {
                    title: '📝 Editar Categoría de Ingreso',
                    description: 'Usa este botón para modificar el nombre, icono o configuración de una categoría de ingreso.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#categories-income-delete-button',
                popover: {
                    title: '🗑️ Eliminar Categoría de Ingreso',
                    description: 'Elimina categorías de ingreso que ya no necesites. Esta acción puede afectar registros existentes.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#btn-help',
                popover: {
                    title: '❓ Boton de ayuda',
                    description: '¿Necesitas volver a ver el tutorial? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-home',
                popover: {
                    title: '🏠 Volver a Bienvenida',
                    description: '¿Necesitas ir a la pantalla de bienvenida? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-finn',
                popover: {
                    title: '🤖 Boton de Finn',
                    description: '¿Necesitas ayuda de Finn? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            }
        ];

        // Filtrar solo los pasos cuyos elementos existen en el DOM
        const availableSteps = allSteps.filter(step => {
            // El paso sin elemento (bienvenida) siempre se incluye
            if (!step.element) return true;

            // Solo incluir si el elemento existe en el DOM
            return document.querySelector(step.element) !== null;
        });

        this.driver.setSteps(availableSteps);
        this.driver.drive();
    },

    // Tour de Gastos e Ingresos
    startExpensesIncomesTour: function () {
        const allSteps = [
            {
                popover: {
                    title: '💸 Gastos e Ingresos',
                    description: 'Aquí puedes registrar todos tus gastos e ingresos, generar reportes y ver tu historial de transacciones.',
                }
            },
            {
                element: '#expenses-add-expense-button',
                popover: {
                    title: '➕ Agregar Gasto',
                    description: 'Haz clic aquí para registrar un nuevo gasto. <strong>Recuerda:</strong> Primero necesitas crear una categoría de gasto en la sección de Categorías.',
                    side: 'bottom',
                    align: 'start'
                }
            },
            {
                element: '#expenses-add-income-button',
                popover: {
                    title: '➕ Agregar Ingreso',
                    description: 'Haz clic aquí para registrar un nuevo ingreso. <strong>Recuerda:</strong> Primero necesitas crear una categoría de ingreso en la sección de Categorías.',
                    side: 'bottom',
                    align: 'start'
                }
            },
            {
                element: '#expenses-balance-cards',
                popover: {
                    title: '💰 Resumen Financiero',
                    description: 'Aquí ves tu balance actual: total de ingresos, adquirido en prestamos, total de gastos y el balance resultante. Tomar en cuenta que los prestamos son tipo "Ajuste"; agregando saldo a tu balance general, pero no a tus ingresos.',
                    side: 'bottom',
                    align: 'center'
                }
            },
            {
                element: '#expenses-report-panel',
                popover: {
                    title: '📊 Generar Reporte',
                    description: 'Filtra tus transacciones por período, categoría y tipo para generar reportes personalizados.',
                    side: 'bottom',
                    align: 'center'
                }
            },
            {
                element: '#expenses-download-pdf',
                popover: {
                    title: '📄 Descargar PDF',
                    description: 'Descarga un reporte en formato PDF con los filtros que hayas seleccionado.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#expenses-download-excel',
                popover: {
                    title: '📊 Descargar Excel',
                    description: 'Descarga un reporte en formato Excel para análisis más detallado.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#expenses-history-table',
                popover: {
                    title: '📋 Historial de Transacciones',
                    description: 'Aquí se mostrarán todas tus transacciones registradas. Podrás verlas, editarlas y eliminarlas.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#expenses-pagination-container',
                popover: {
                    title: '📄 Controles de Paginación',
                    description: 'Navega entre las páginas de tus transacciones y personaliza cuántos registros quieres ver.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#expenses-page-size-selector',
                popover: {
                    title: '🔢 Registros por Página',
                    description: 'Selecciona cuántas transacciones quieres ver en cada página (5, 10, 25, 50 o 100).',
                    side: 'top',
                    align: 'start'
                }
            },
            {
                element: '#expenses-previous-button',
                popover: {
                    title: '⬅️ Página Anterior',
                    description: 'Navega a la página anterior de transacciones.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#expenses-next-button',
                popover: {
                    title: '➡️ Página Siguiente',
                    description: 'Navega a la siguiente página de transacciones.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#expenses-edit-button',
                popover: {
                    title: '📝 Editar Transacción',
                    description: 'Haz clic en este botón para modificar los datos de una transacción.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#expenses-delete-button',
                popover: {
                    title: '🗑️ Eliminar Transacción',
                    description: 'Haz clic en este botón para eliminar una transacción. Se te pedirá confirmación antes de eliminarla.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#btn-help',
                popover: {
                    title: '❓ Boton de ayuda',
                    description: '¿Necesitas volver a ver el tutorial? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-home',
                popover: {
                    title: '🏠 Volver a Bienvenida',
                    description: '¿Necesitas ir a la pantalla de bienvenida? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-finn',
                popover: {
                    title: '🤖 Boton de Finn',
                    description: '¿Necesitas ayuda de Finn? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            }
        ];

        // Filtrado dinámico: El step de la tabla (#expenses-history-table) SIEMPRE se muestra
        const availableSteps = allSteps.filter(step => {
            if (!step.element) return true; // Paso de bienvenida siempre
            if (step.element === '#expenses-history-table') return true; // Tabla SIEMPRE se muestra
            return document.querySelector(step.element) !== null;
        });

        this.init();
        this.driver.setSteps(availableSteps);
        this.driver.drive();
    },

    // Tour de Recordatorios
    startRemindersTour: function () {
        const allSteps = [
            {
                popover: {
                    title: '🔔 Recordatorios',
                    description: 'Aquí puedes ver y gestionar todos los recordatorios de tus gastos recurrentes. El sistema te notifica cuando se acerca la fecha de vencimiento de un gasto.',
                }
            },
            {
                element: '#reminders-filters',
                popover: {
                    title: '🔍 Filtros de Recordatorios',
                    description: 'Filtra tus recordatorios para ver todos, solo los no leídos o solo los leídos.',
                    side: 'bottom',
                    align: 'start'
                }
            },
            {
                element: '#reminders-mark-all-button',
                popover: {
                    title: '✓ Marcar Todos como Leídos',
                    description: 'Si tienes varios recordatorios sin leer, puedes marcarlos todos como leídos de una vez con este botón.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#reminders-list',
                popover: {
                    title: '📋 Lista de Recordatorios',
                    description: 'Aquí se muestran todos tus recordatorios con información sobre la categoría, fecha de vencimiento, monto estimado y estado.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#reminders-status-badge',
                popover: {
                    title: '⏰ Estado del Recordatorio',
                    description: 'Este badge te indica el estado del recordatorio: "Vencido" (ya pasó la fecha), "Vence hoy" (es hoy) o "Vence en X días" (próximamente).',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#reminders-mark-read-button',
                popover: {
                    title: '✓ Marcar como Leído',
                    description: 'Haz clic aquí para marcar este recordatorio como leído. Solo aparece en recordatorios no leídos.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#reminders-delete-button',
                popover: {
                    title: '🗑️ Eliminar Recordatorio',
                    description: 'Haz clic aquí para eliminar este recordatorio de tu lista.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#btn-help',
                popover: {
                    title: '❓ Boton de ayuda',
                    description: '¿Necesitas volver a ver el tutorial? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-home',
                popover: {
                    title: '🏠 Volver a Bienvenida',
                    description: '¿Necesitas ir a la pantalla de bienvenida? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            }
        ];

        // Filtrado dinámico: mostrar solo los steps con elementos visibles
        const availableSteps = allSteps.filter(step => {
            if (!step.element) return true; // Paso de bienvenida siempre
            return document.querySelector(step.element) !== null;
        });

        this.init();
        this.driver.setSteps(availableSteps);
        this.driver.drive();
    },

    // Tour de Metas
    startGoalsTour: function () {
        this.init();

        // Definir todos los pasos posibles
        const allSteps = [
            {
                popover: {
                    title: '🎯 Metas Financieras',
                    description: 'Define objetivos de ahorro y sigue tu progreso hacia ellos. Te guiaré por las funciones principales.',
                }
            },
            {
                element: '#goals-add-button',
                popover: {
                    title: '➕ Crear Nueva Meta',
                    description: 'Haz clic aquí para establecer una nueva meta de ahorro con monto objetivo y fecha límite.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#goals-balance-card',
                popover: {
                    title: '💵 Balance Disponible',
                    description: 'Visualiza tu balance disponible, el dinero comprometido en metas, y el total de ingresos y gastos.',
                    side: 'bottom',
                    align: 'start'
                }
            },
            {
                element: '#goals-filters',
                popover: {
                    title: '🔍 Filtrar Metas',
                    description: 'Filtra entre metas en progreso, completadas, canceladas o ver todas a la vez.',
                    side: 'bottom',
                    align: 'center'
                }
            },
            {
                element: '#goals-list',
                popover: {
                    title: '📋 Lista de Metas',
                    description: 'Aquí verás todas tus metas con su progreso, fechas y opciones de gestión.',
                    side: 'top',
                    align: 'start'
                }
            },
            {
                element: '#goals-add-progress-button',
                popover: {
                    title: '➕ Agregar Progreso',
                    description: 'Añade dinero a tu meta para acercarte a tu objetivo de ahorro.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#goals-withdraw-button',
                popover: {
                    title: '➖ Retirar Dinero',
                    description: 'Retira dinero de una meta en caso de necesidad o cuando la hayas completado.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#goals-edit-button',
                popover: {
                    title: '📝 Editar Meta',
                    description: 'Modifica el nombre, monto objetivo o fecha límite de una meta activa.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#goals-cancel-button',
                popover: {
                    title: '❌ Cancelar Meta',
                    description: 'Cancela una meta que ya no deseas seguir. Podrás reactivarla después si lo necesitas.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#goals-reactivate-button',
                popover: {
                    title: '🔄 Reactivar Meta',
                    description: 'Vuelve a activar una meta cancelada para continuar trabajando en ella.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#goals-delete-button',
                popover: {
                    title: '🗑️ Eliminar Meta',
                    description: 'Elimina permanentemente una meta. Esta acción no se puede deshacer.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#btn-help',
                popover: {
                    title: '❓ Boton de ayuda',
                    description: '¿Necesitas volver a ver el tutorial? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-home',
                popover: {
                    title: '🏠 Volver a Bienvenida',
                    description: '¿Necesitas ir a la pantalla de bienvenida? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-finn',
                popover: {
                    title: '🤖 Boton de Finn',
                    description: '¿Necesitas ayuda de Finn? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            }
        ];

        // Filtrar solo los pasos cuyos elementos existen en el DOM
        const availableSteps = allSteps.filter(step => {
            // El paso sin elemento (bienvenida) siempre se incluye
            if (!step.element) return true;

            // Solo incluir si el elemento existe en el DOM
            return document.querySelector(step.element) !== null;
        });

        this.driver.setSteps(availableSteps);
        this.driver.drive();
    },

    // Tour de Préstamos
    startLoansTour: function () {
        const allSteps = [
            {
                popover: {
                    title: '🏦 Préstamos',
                    description: 'Aquí puedes gestionar todos tus préstamos, registrar pagos y dar seguimiento a tus cuotas.',
                }
            },
            {
                element: '#loans-add-button',
                popover: {
                    title: '➕ Nuevo Préstamo',
                    description: 'Haz clic aquí para registrar un nuevo préstamo. Podrás agregar el monto, plazo, interés y otros detalles.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#loans-summary-card',
                popover: {
                    title: '📊 Resumen General',
                    description: 'Aquí ves el resumen de todos tus préstamos: total prestado, total a pagar, total pagado, deuda pendiente, cuotas mensuales y tasa promedio.',
                    side: 'bottom',
                    align: 'center'
                }
            },
            {
                element: '#loans-filters',
                popover: {
                    title: '🔍 Filtros',
                    description: 'Filtra tus préstamos para ver solo los activos o todos (incluyendo completados y cancelados).',
                    side: 'bottom',
                    align: 'center'
                }
            },
            {
                element: '#loans-list',
                popover: {
                    title: '📋 Lista de Préstamos',
                    description: 'Aquí se muestran todos tus préstamos con su información detallada, progreso de pagos y acciones disponibles.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#loans-register-payment-button',
                popover: {
                    title: '💵 Registrar Pago',
                    description: 'Haz clic aquí para registrar el pago de una cuota del préstamo.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#loans-undo-payment-button',
                popover: {
                    title: '↩️ Deshacer Último Pago',
                    description: 'Si registraste un pago por error, puedes deshacerlo con este botón. Solo aparece si hay al menos un pago registrado.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#loans-balance-abjusment-button',
                popover: {
                    title: '⚖️ Ajustar balance real',
                    description: 'Haz clic aquí para ajustar el balance del préstamo en la app con el balance del banco sí es necesario.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#loans-edit-button',
                popover: {
                    title: '📝 Editar Préstamo',
                    description: 'Haz clic aquí para modificar los datos del préstamo.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#loans-reactivate-button',
                popover: {
                    title: '🔄 Reactivar Préstamo',
                    description: 'Si cancelaste un préstamo, puedes reactivarlo con este botón para continuar registrando pagos.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#loans-delete-button',
                popover: {
                    title: '🗑️ Eliminar Préstamo',
                    description: 'Haz clic aquí para eliminar el préstamo. Si está activo, se cancelará primero antes de eliminarse del historial.',
                    side: 'left',
                    align: 'center'
                }
            },
            {
                element: '#btn-help',
                popover: {
                    title: '❓ Boton de ayuda',
                    description: '¿Necesitas volver a ver el tutorial? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-home',
                popover: {
                    title: '🏠 Volver a Bienvenida',
                    description: '¿Necesitas ir a la pantalla de bienvenida? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-finn',
                popover: {
                    title: '🤖 Boton de Finn',
                    description: '¿Necesitas ayuda de Finn? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            }
        ];

        // Filtrado dinámico: mostrar solo los steps con elementos visibles
        const availableSteps = allSteps.filter(step => {
            if (!step.element) return true; // Paso de bienvenida siempre
            return document.querySelector(step.element) !== null;
        });

        this.init();
        this.driver.setSteps(availableSteps);
        this.driver.drive();
    },

    // Tour de Home
    startHomeTour: function () {
        const allSteps = [
            {
                popover: {
                    title: '👋 ¡Bienvenido a Mis Finanzas!',
                    description: 'Esta es tu aplicación de gestión financiera personal. Te ayudaremos a tomar el control de tu dinero con inteligencia, simplicidad, y la ayuda de 🤖 Finn; tu asistente financiero personal.',
                }
            },
            {
                element: '#home-register-button',
                popover: {
                    title: '📝 Registrarse',
                    description: 'Si aún no tienes una cuenta, haz clic aquí para crear una y comenzar a gestionar tus finanzas.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#home-login-button',
                popover: {
                    title: '🔐 Iniciar Sesión',
                    description: 'Si ya tienes una cuenta, ingresa aquí con tus credenciales.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#home-dashboard-button',
                popover: {
                    title: '🚀 Ir a Control',
                    description: 'Accede directamente a tu panel de control para gestionar tus finanzas.',
                    side: 'bottom',
                    align: 'end'
                }
            },
            {
                element: '#home-categories-card',
                popover: {
                    title: '🏷️ Categorías',
                    description: 'Organiza tus finanzas por categorías personalizadas. Crea categorías para alimentación, transporte, entretenimiento y más.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#home-budgets-card',
                popover: {
                    title: '💰 Presupuestos',
                    description: 'Controla tus gastos mensuales por categoría. Establece límites y visualiza tu progreso en tiempo real.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#home-transactions-card',
                popover: {
                    title: '💳 Gastos e ingresos',
                    description: 'Registra y visualiza cada movimiento de dinero. Lleva un control detallado de todos tus gastos e ingresos. <strong>Recuerda:</strong> Primero necesitas crear una categoría de gasto o ingreso en la sección de Categorías.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#home-reminders-card',
                popover: {
                    title: '🔔 Recordatorios',
                    description: 'Recibe notificaciones de gastos fijos y pagos pendientes. Nunca olvides un pago importante.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#home-goals-card',
                popover: {
                    title: '🎯 Metas financieras',
                    description: 'Define objetivos y alcanza tus sueños. Ahorra para vacaciones, compras importantes o emergencias.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#home-loans-card',
                popover: {
                    title: '🏦 Préstamos',
                    description: 'Gestiona tus préstamos y cuotas mensuales. Lleva un control de tus deudas y pagos.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#home-dashboard-card',
                popover: {
                    title: '📊 Panel de Control',
                    description: 'Visualiza tu situación financiera de un vistazo. Gráficos, estadísticas y resúmenes de tu dinero.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#home-download-manual-button',
                popover: {
                    title: '📥 Manual de Usuario',
                    description: 'Descarga una guía completa sobre cómo usar todas las funcionalidades de la aplicación.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#home-contact-card',
                popover: {
                    title: '📞 Contacto',
                    description: '¿Necesitas ayuda? Contáctanos por WhatsApp o email. Estamos aquí para asistirte.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-help',
                popover: {
                    title: '❓ Boton de ayuda',
                    description: '¿Necesitas volver a ver el tutorial? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            }
        ];

        // Filtrado dinámico: mostrar solo los steps con elementos visibles
        const availableSteps = allSteps.filter(step => {
            if (!step.element) return true; // Paso de bienvenida siempre
            return document.querySelector(step.element) !== null;
        });

        this.init();
        this.driver.setSteps(availableSteps);
        this.driver.drive();
    },

    // Tour de Profile
    startProfileTour: function () {
        const allSteps = [
            {
                popover: {
                    title: '👤 Mi Perfil',
                    description: 'Aquí puedes gestionar tu información personal, cambiar tu contraseña y administrar tu cuenta.',
                }
            },
            {
                element: '#profile-info-card',
                popover: {
                    title: '📋 Información Personal',
                    description: 'Visualiza tu nombre de usuario, correo electrónico y rol en el sistema.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#profile-edit-info-button',
                popover: {
                    title: '✏️ Editar Información',
                    description: 'Haz clic aquí para modificar tu correo electrónico. El nombre de usuario no se puede cambiar.',
                    side: 'bottom',
                    align: 'center'
                }
            },
            {
                element: '#profile-password-card',
                popover: {
                    title: '🔐 Cambiar Contraseña',
                    description: 'Actualiza tu contraseña para mantener tu cuenta segura. Necesitarás ingresar tu contraseña actual.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#profile-change-password-button',
                popover: {
                    title: '🔒 Guardar Nueva Contraseña',
                    description: 'Una vez que hayas completado los campos, haz clic aquí para cambiar tu contraseña.',
                    side: 'bottom',
                    align: 'center'
                }
            },
            {
                element: '#profile-danger-zone-card',
                popover: {
                    title: '⚠️ Zona de Peligro',
                    description: 'Esta sección contiene acciones irreversibles. Eliminar tu cuenta borrará permanentemente todos tus datos.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#profile-delete-account-button',
                popover: {
                    title: '🗑️ Eliminar Cuenta',
                    description: '<strong>¡CUIDADO!</strong> Esta acción no se puede deshacer. Perderás todos tus datos: categorías, transacciones, metas y presupuestos.',
                    side: 'bottom',
                    align: 'center'
                }
            },
            {
                element: '#btn-help',
                popover: {
                    title: '❓ Boton de ayuda',
                    description: '¿Necesitas volver a ver el tutorial? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            },
            {
                element: '#btn-home',
                popover: {
                    title: '🏠 Volver a Bienvenida',
                    description: '¿Necesitas ir a la pantalla de bienvenida? Clic aqui.',
                    side: 'top',
                    align: 'center'
                }
            }
        ];

        // Filtrado dinámico: mostrar solo los steps con elementos visibles
        const availableSteps = allSteps.filter(step => {
            if (!step.element) return true; // Paso de bienvenida siempre
            return document.querySelector(step.element) !== null;
        });

        this.init();
        this.driver.setSteps(availableSteps);
        this.driver.drive();
    },

    // Detener tour
    stop: function () {
        if (this.driver) {
            this.driver.destroy();
        }
    },

    // Verificar si el tour ya fue visto
    hasTourBeenSeen: function (tourName) {
        const seenTours = JSON.parse(localStorage.getItem('seenTours') || '[]');
        return seenTours.includes(tourName);
    },

    // Marcar tour como visto
    markTourAsSeen: function (tourName) {
        const seenTours = JSON.parse(localStorage.getItem('seenTours') || '[]');
        if (!seenTours.includes(tourName)) {
            seenTours.push(tourName);
            localStorage.setItem('seenTours', JSON.stringify(seenTours));
        }
    },

    // Iniciar tour automáticamente si no ha sido visto
    startTourIfFirstTime: function (tourName) {
        if (!this.hasTourBeenSeen(tourName)) {
            // Iniciar el tour correspondiente
            switch (tourName) {
                case 'dashboard':
                    this.startDashboardTour();
                    break;
                case 'categories':
                    this.startCategoriesTour();
                    break;
                case 'budgets':
                    this.startBudgetsTour();
                    break;
                case 'expenses':
                    this.startExpensesIncomesTour();
                    break;
                case 'goals':
                    this.startGoalsTour();
                    break;
                case 'loans':
                    this.startLoansTour();
                    break;
                case 'reminders':
                    this.startRemindersTour();
                    break;
                case 'home':
                    this.startHomeTour();
                    break;
                case 'profile':
                    this.startProfileTour();
                    break;
            }
            // Marcar como visto
            this.markTourAsSeen(tourName);
        }
    },

    // Resetear todos los tours (útil para testing)
    resetAllTours: function () {
        localStorage.removeItem('seenTours');
        console.log('✅ Todos los tours han sido reseteados');
    },
    // NUEVO: Verificar si hay un tour corriendo actualmente
    isTourActive: function () {
        // Driver.js agrega la clase 'driver-active' al body cuando corre
        // También verificamos si existe el elemento del popover por si acaso
        return document.body.classList.contains('driver-active') ||
            document.querySelector('.driver-popover') !== null;
    }
};