// Helper para crear gráficos con Chart.js

window.chartHelpers = {
    // Crear gráfico de barras (Bar Chart)
    createBarChart: function (canvasId, labels, incomeData, expenseData) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return null;

        // Destruir gráfico anterior si existe
        if (ctx.chart) {
            ctx.chart.destroy();
        }

        const chart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Ingresos',
                        data: incomeData,
                        backgroundColor: 'rgba(16, 185, 129, 0.8)',
                        borderColor: 'rgba(16, 185, 129, 1)',
                        borderWidth: 2,
                        borderRadius: 8,
                        borderSkipped: false,
                    },
                    {
                        label: 'Gastos',
                        data: expenseData,
                        backgroundColor: 'rgba(239, 68, 68, 0.8)',
                        borderColor: 'rgba(239, 68, 68, 1)',
                        borderWidth: 2,
                        borderRadius: 8,
                        borderSkipped: false,
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: 'index',
                },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: {
                            padding: 15,
                            font: {
                                size: 12,
                                family: "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif"
                            },
                            usePointStyle: true,
                            pointStyle: 'circle',
                            color: '#fff'
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        padding: 12,
                        cornerRadius: 8,
                        titleFont: {
                            size: 14,
                            weight: 'bold'
                        },
                        bodyFont: {
                            size: 13
                        },
                        callbacks: {
                            label: function (context) {
                                let label = context.dataset.label || '';
                                if (label) {
                                    label += ': ';
                                }
                                label += 'RD$' + context.parsed.y.toLocaleString('es-DO');
                                return label;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(255, 255, 255, 0.1)',
                            drawBorder: false
                        },
                        ticks: {
                            callback: function (value) {
                                return 'RD$' + value.toLocaleString('es-DO');
                            },
                            font: {
                                size: 11
                            },
                            color: '#fff'
                        }
                    },
                    x: {
                        grid: {
                            display: false,
                            drawBorder: false
                        },
                        ticks: {
                            font: {
                                size: 11
                            },
                            color: '#fff'
                        }
                    }
                },
                animation: {
                    duration: 1500,
                    easing: 'easeInOutQuart',
                    delay: (context) => {
                        let delay = 0;
                        if (context.type === 'data' && context.mode === 'default') {
                            delay = context.dataIndex * 100 + context.datasetIndex * 50;
                        }
                        return delay;
                    }
                }
            }
        });

        ctx.chart = chart;
        return chart;
    },

    // Crear gráfico de dona para distribución de gastos (Doughnut)
    createDoughnutChart: function (canvasId, labels, data, backgroundColors) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return null;

        // Destruir gráfico anterior si existe
        if (ctx.chart) {
            ctx.chart.destroy();
        }

        const chart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: backgroundColors,
                    borderWidth: 0,
                    hoverOffset: 15
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '70%',
                plugins: {
                    legend: {
                        display: true,
                        position: 'bottom',
                        labels: {
                            padding: 15,
                            font: {
                                size: 12,
                                family: "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif"
                            },
                            usePointStyle: true,
                            pointStyle: 'circle',
                            color: '#fff'
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        padding: 12,
                        cornerRadius: 8,
                        titleFont: {
                            size: 14,
                            weight: 'bold'
                        },
                        bodyFont: {
                            size: 13
                        },
                        callbacks: {
                            label: function (context) {
                                let label = context.label || '';
                                if (label) {
                                    label += ': ';
                                }
                                const value = context.parsed;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                label += 'RD$' + value.toLocaleString('es-DO') + ' (' + percentage + '%)';
                                return label;
                            }
                        }
                    }
                },
                animation: {
                    animateRotate: true,
                    animateScale: true,
                    duration: 1500,
                    easing: 'easeInOutQuart'
                }
            }
        });

        ctx.chart = chart;
        return chart;
    },

    // Crear gráfico de dona para presupuestos (Doughnut)
    createBudgetChart: function (canvasId, labels, values, colors) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return null;

        // Destruir gráfico anterior si existe
        if (ctx.chart) {
            ctx.chart.destroy();
        }

        const chart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: values,
                    backgroundColor: colors,
                    borderColor: 'rgba(255, 255, 255, 0.1)',
                    borderWidth: 2,
                    hoverBorderColor: '#fff',
                    hoverBorderWidth: 3,
                    hoverOffset: 10
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '65%',
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: 'rgba(255, 255, 255, 0.2)',
                        borderWidth: 1,
                        padding: 12,
                        cornerRadius: 8,
                        callbacks: {
                            label: function (context) {
                                let label = context.label || '';
                                if (label) {
                                    label += ': ';
                                }
                                label += context.parsed.toFixed(0) + '% usado';
                                return label;
                            }
                        }
                    }
                },
                animation: {
                    animateRotate: true,
                    animateScale: true,
                    duration: 1500,
                    easing: 'easeInOutQuart'
                }
            }
        });

        ctx.chart = chart;
        return chart;
    },

    // Destruir gráfico
    destroyChart: function (canvasId) {
        const ctx = document.getElementById(canvasId);
        if (ctx && ctx.chart) {
            ctx.chart.destroy();
            ctx.chart = null;
        }
    }
};

console.log('✅ Dashboard charts helpers loaded');