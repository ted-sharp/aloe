/**
 * Medock Calendar Canvas - High Performance Calendar Renderer
 * 
 * All rendering and event handling is done in JavaScript to minimize JSInterop overhead.
 * C# only sends data and receives action callbacks.
 */

window.MedockCalendar = (function () {
    'use strict';

    // ============================================================
    // Configuration & Constants
    // ============================================================
    const CONFIG = {
        colors: {
            am: { empty: '#e5e7eb', filled: '#10b981', text: '#065f46' },
            pm: { empty: '#e5e7eb', filled: '#3b82f6', text: '#1e40af' },
            status: {
                0: '#fbbf24', // Reserved (warning)
                1: '#60a5fa', // Waiting (info)
                2: '#34d399', // Visited (success)
                3: '#f87171'  // Cancelled (error)
            },
            grid: '#d1d5db',
            today: '#10b981',
            weekend: { sun: '#ef4444', sat: '#3b82f6' },
            hover: 'rgba(16, 185, 129, 0.1)'
        },
        font: {
            family: 'system-ui, -apple-system, sans-serif',
            sizeSmall: 10,
            sizeMedium: 12,
            sizeLarge: 14
        },
        animation: {
            duration: 150
        }
    };

    // ============================================================
    // State Management
    // ============================================================
    let state = {
        stage: null,
        layers: {},
        dotNetRef: null,
        containerId: null,
        currentView: 'month', // 'year', 'month', 'week'
        currentDate: new Date(),
        appointments: [],
        dayStats: new Map(), // date string -> { am: count, pm: count, total: count }
        options: {
            weekDays: 7, // 1, 3, 7, 14, 31
            showSlots: true,
            startHour: 8,
            endHour: 18
        },
        tooltip: null,
        hoveredElement: null
    };

    // ============================================================
    // Utility Functions
    // ============================================================
    function dateToString(date) {
        if (typeof date === 'string') return date.split('T')[0];
        const d = new Date(date);
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    }

    function parseDate(dateStr) {
        const [year, month, day] = dateStr.split('-').map(Number);
        return new Date(year, month - 1, day);
    }

    function isToday(date) {
        const today = new Date();
        return dateToString(date) === dateToString(today);
    }

    function isWeekend(date) {
        const d = new Date(date);
        return d.getDay() === 0 || d.getDay() === 6;
    }

    function getDayOfWeek(date) {
        return new Date(date).getDay();
    }

    function getFirstDayOfMonth(year, month) {
        return new Date(year, month, 1);
    }

    function getLastDayOfMonth(year, month) {
        return new Date(year, month + 1, 0);
    }

    function getStartOfWeek(date) {
        const d = new Date(date);
        const day = d.getDay();
        d.setDate(d.getDate() - day);
        return d;
    }

    // ============================================================
    // D3.js Arc Calculations for AM/PM Pie Charts
    // ============================================================
    function createArcPath(startAngle, endAngle, innerRadius, outerRadius) {
        const arc = d3.arc()
            .innerRadius(innerRadius)
            .outerRadius(outerRadius)
            .startAngle(startAngle)
            .endAngle(endAngle);
        return arc();
    }

    function calculatePieData(amRatio, pmRatio) {
        // AM: left half (-PI/2 to PI/2)
        // PM: right half (PI/2 to 3*PI/2)
        return {
            am: {
                filled: { start: -Math.PI / 2, end: -Math.PI / 2 + Math.PI * amRatio },
                empty: { start: -Math.PI / 2 + Math.PI * amRatio, end: Math.PI / 2 }
            },
            pm: {
                filled: { start: Math.PI / 2, end: Math.PI / 2 + Math.PI * pmRatio },
                empty: { start: Math.PI / 2 + Math.PI * pmRatio, end: 3 * Math.PI / 2 }
            }
        };
    }

    // ============================================================
    // Tooltip Management
    // ============================================================
    function createTooltip() {
        const container = document.getElementById(state.containerId);
        if (!container) return;

        let tooltip = container.querySelector('.medock-tooltip');
        if (!tooltip) {
            tooltip = document.createElement('div');
            tooltip.className = 'medock-tooltip';
            tooltip.style.cssText = `
                position: absolute;
                background: rgba(0, 0, 0, 0.85);
                color: white;
                padding: 8px 12px;
                border-radius: 6px;
                font-size: 12px;
                pointer-events: none;
                z-index: 1000;
                opacity: 0;
                transition: opacity 0.15s ease;
                max-width: 250px;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            `;
            container.appendChild(tooltip);
        }
        state.tooltip = tooltip;
    }

    function showTooltip(x, y, content) {
        if (!state.tooltip) return;
        state.tooltip.innerHTML = content;
        state.tooltip.style.left = `${x + 10}px`;
        state.tooltip.style.top = `${y + 10}px`;
        state.tooltip.style.opacity = '1';
    }

    function hideTooltip() {
        if (!state.tooltip) return;
        state.tooltip.style.opacity = '0';
    }

    // ============================================================
    // Layer Management
    // ============================================================
    function createLayers() {
        // Clear existing layers
        if (state.layers.background) state.layers.background.destroy();
        if (state.layers.grid) state.layers.grid.destroy();
        if (state.layers.content) state.layers.content.destroy();
        if (state.layers.interaction) state.layers.interaction.destroy();

        state.layers = {
            background: new Konva.Layer(),
            grid: new Konva.Layer(),
            content: new Konva.Layer(),
            interaction: new Konva.Layer()
        };

        state.stage.add(state.layers.background);
        state.stage.add(state.layers.grid);
        state.stage.add(state.layers.content);
        state.stage.add(state.layers.interaction);
    }

    // ============================================================
    // Year View Renderer
    // ============================================================
    function renderYearView() {
        const { stage, layers, currentDate, dayStats } = state;
        const width = stage.width();
        const height = stage.height();

        // Clear layers
        layers.grid.destroyChildren();
        layers.content.destroyChildren();
        layers.interaction.destroyChildren();

        const year = currentDate.getFullYear();
        const cols = 4;
        const rows = 3;
        const monthWidth = width / cols;
        const monthHeight = height / rows;
        const padding = 10;

        for (let month = 0; month < 12; month++) {
            const col = month % cols;
            const row = Math.floor(month / cols);
            const x = col * monthWidth + padding;
            const y = row * monthHeight + padding;
            const w = monthWidth - padding * 2;
            const h = monthHeight - padding * 2;

            renderMonthMiniCalendar(month, x, y, w, h, year);
        }

        layers.grid.batchDraw();
        layers.content.batchDraw();
        layers.interaction.batchDraw();
    }

    function renderMonthMiniCalendar(month, x, y, width, height, year) {
        const { layers, dayStats } = state;
        const monthNames = ['1月', '2月', '3月', '4月', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月'];

        // Month header
        const header = new Konva.Text({
            x: x,
            y: y,
            width: width,
            text: monthNames[month],
            fontSize: CONFIG.font.sizeLarge,
            fontFamily: CONFIG.font.family,
            fontStyle: 'bold',
            fill: '#374151',
            align: 'center'
        });
        layers.grid.add(header);

        // Day grid
        const gridTop = y + 25;
        const gridHeight = height - 30;
        const cellWidth = width / 7;
        const firstDay = getFirstDayOfMonth(year, month);
        const lastDay = getLastDayOfMonth(year, month);
        const startDayOfWeek = firstDay.getDay();
        const daysInMonth = lastDay.getDate();
        const rows = Math.ceil((startDayOfWeek + daysInMonth) / 7);
        const cellHeight = gridHeight / (rows + 1);

        // Day of week headers
        const dayNames = ['日', '月', '火', '水', '木', '金', '土'];
        for (let i = 0; i < 7; i++) {
            const dayHeader = new Konva.Text({
                x: x + i * cellWidth,
                y: gridTop,
                width: cellWidth,
                text: dayNames[i],
                fontSize: CONFIG.font.sizeSmall,
                fontFamily: CONFIG.font.family,
                fill: i === 0 ? CONFIG.colors.weekend.sun : i === 6 ? CONFIG.colors.weekend.sat : '#6b7280',
                align: 'center'
            });
            layers.grid.add(dayHeader);
        }

        // Day cells with pie charts
        let day = 1;
        for (let row = 0; row < rows; row++) {
            for (let col = 0; col < 7; col++) {
                const cellIndex = row * 7 + col;
                if (cellIndex < startDayOfWeek || day > daysInMonth) continue;

                const cellX = x + col * cellWidth + cellWidth / 2;
                const cellY = gridTop + (row + 1) * cellHeight + cellHeight / 2;
                const radius = Math.min(cellWidth, cellHeight) / 2 - 2;

                const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
                const stats = dayStats.get(dateStr) || { am: 0, pm: 0, amMax: 10, pmMax: 10 };
                const amRatio = stats.amMax > 0 ? stats.am / stats.amMax : 0;
                const pmRatio = stats.pmMax > 0 ? stats.pm / stats.pmMax : 0;

                renderDayPieChart(cellX, cellY, radius, amRatio, pmRatio, dateStr, day);

                day++;
            }
        }
    }

    function renderDayPieChart(cx, cy, radius, amRatio, pmRatio, dateStr, dayNumber) {
        const { layers } = state;
        const innerRadius = radius * 0.55; // ドーナツの内側半径

        // Background circle (outer ring)
        const bgCircle = new Konva.Circle({
            x: cx,
            y: cy,
            radius: radius,
            fill: '#e5e7eb',
            stroke: isToday(dateStr) ? CONFIG.colors.today : '#d1d5db',
            strokeWidth: isToday(dateStr) ? 2 : 1
        });
        layers.content.add(bgCircle);

        // AM filled arc (donut style)
        if (amRatio > 0) {
            const amArc = new Konva.Arc({
                x: cx,
                y: cy,
                innerRadius: innerRadius,
                outerRadius: radius - 1,
                angle: 180 * amRatio,
                rotation: -90,
                fill: CONFIG.colors.am.filled,
                opacity: 0.9
            });
            layers.content.add(amArc);
        }

        // PM filled arc (donut style)
        if (pmRatio > 0) {
            const pmArc = new Konva.Arc({
                x: cx,
                y: cy,
                innerRadius: innerRadius,
                outerRadius: radius - 1,
                angle: 180 * pmRatio,
                rotation: 90,
                fill: CONFIG.colors.pm.filled,
                opacity: 0.9
            });
            layers.content.add(pmArc);
        }

        // Center white circle for text background
        const centerCircle = new Konva.Circle({
            x: cx,
            y: cy,
            radius: innerRadius - 1,
            fill: 'white',
            stroke: '#e5e7eb',
            strokeWidth: 0.5
        });
        layers.content.add(centerCircle);

        // Day number text (centered in white circle)
        const text = new Konva.Text({
            x: cx - innerRadius,
            y: cy - CONFIG.font.sizeSmall / 2,
            width: innerRadius * 2,
            text: String(dayNumber),
            fontSize: CONFIG.font.sizeSmall,
            fontFamily: CONFIG.font.family,
            fontStyle: isToday(dateStr) ? 'bold' : 'normal',
            fill: isToday(dateStr) ? CONFIG.colors.today : '#374151',
            align: 'center'
        });
        layers.content.add(text);

        // Interaction rect
        const hitArea = new Konva.Rect({
            x: cx - radius,
            y: cy - radius,
            width: radius * 2,
            height: radius * 2,
            fill: 'transparent'
        });
        hitArea.on('mouseenter', function (e) {
            const stats = state.dayStats.get(dateStr) || { am: 0, pm: 0 };
            showTooltip(e.evt.clientX, e.evt.clientY,
                `<strong>${dateStr}</strong><br>午前: ${stats.am}件<br>午後: ${stats.pm}件`);
            bgCircle.stroke(CONFIG.colors.today);
            layers.content.batchDraw();
        });
        hitArea.on('mouseleave', function () {
            hideTooltip();
            bgCircle.stroke(isToday(dateStr) ? CONFIG.colors.today : '#e5e7eb');
            layers.content.batchDraw();
        });
        hitArea.on('click', function () {
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateSelected', dateStr);
            }
        });
        layers.interaction.add(hitArea);
    }

    // ============================================================
    // Month View Renderer
    // ============================================================
    function renderMonthView() {
        const { stage, layers, currentDate, dayStats } = state;
        const width = stage.width();
        const height = stage.height();

        layers.grid.destroyChildren();
        layers.content.destroyChildren();
        layers.interaction.destroyChildren();

        const year = currentDate.getFullYear();
        const month = currentDate.getMonth();
        const firstDay = getFirstDayOfMonth(year, month);
        const lastDay = getLastDayOfMonth(year, month);
        const startDayOfWeek = firstDay.getDay();
        const daysInMonth = lastDay.getDate();

        const headerHeight = 40;
        const cellWidth = width / 7;
        const rows = Math.ceil((startDayOfWeek + daysInMonth) / 7);
        const cellHeight = (height - headerHeight) / (rows + 1);

        // Day of week headers
        const dayNames = ['日', '月', '火', '水', '木', '金', '土'];
        for (let i = 0; i < 7; i++) {
            // Header background
            const headerBg = new Konva.Rect({
                x: i * cellWidth,
                y: 0,
                width: cellWidth,
                height: headerHeight,
                fill: '#f9fafb',
                stroke: CONFIG.colors.grid,
                strokeWidth: 1
            });
            layers.grid.add(headerBg);

            const dayHeader = new Konva.Text({
                x: i * cellWidth,
                y: 12,
                width: cellWidth,
                text: dayNames[i],
                fontSize: CONFIG.font.sizeLarge,
                fontFamily: CONFIG.font.family,
                fontStyle: 'bold',
                fill: i === 0 ? CONFIG.colors.weekend.sun : i === 6 ? CONFIG.colors.weekend.sat : '#374151',
                align: 'center'
            });
            layers.grid.add(dayHeader);
        }

        // Day cells
        let day = 1;
        for (let row = 0; row < rows; row++) {
            for (let col = 0; col < 7; col++) {
                const cellIndex = row * 7 + col;
                if (cellIndex < startDayOfWeek || day > daysInMonth) {
                    // Empty cell
                    const emptyCell = new Konva.Rect({
                        x: col * cellWidth,
                        y: headerHeight + row * cellHeight,
                        width: cellWidth,
                        height: cellHeight,
                        fill: '#f3f4f6',
                        stroke: CONFIG.colors.grid,
                        strokeWidth: 1
                    });
                    layers.grid.add(emptyCell);
                    continue;
                }

                const cellX = col * cellWidth;
                const cellY = headerHeight + row * cellHeight;
                const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;

                renderMonthDayCell(cellX, cellY, cellWidth, cellHeight, dateStr, day, col);

                day++;
            }
        }

        layers.grid.batchDraw();
        layers.content.batchDraw();
        layers.interaction.batchDraw();
    }

    function renderMonthDayCell(x, y, width, height, dateStr, dayNumber, dayOfWeek) {
        const { layers, dayStats } = state;
        const stats = dayStats.get(dateStr) || { am: 0, pm: 0, amMax: 10, pmMax: 10 };
        const amRatio = stats.amMax > 0 ? stats.am / stats.amMax : 0;
        const pmRatio = stats.pmMax > 0 ? stats.pm / stats.pmMax : 0;
        const total = stats.am + stats.pm;

        // Cell background
        const cellBg = new Konva.Rect({
            x: x,
            y: y,
            width: width,
            height: height,
            fill: isToday(dateStr) ? 'rgba(16, 185, 129, 0.1)' : 'white',
            stroke: CONFIG.colors.grid,
            strokeWidth: 1
        });
        layers.grid.add(cellBg);

        // AM/PM Donut chart centered in cell
        const pieRadius = Math.min(width, height) * 0.35;
        const innerRadius = pieRadius * 0.5;
        const pieCx = x + width / 2;
        const pieCy = y + height / 2;

        // Background circle (outer ring)
        const pieCircle = new Konva.Circle({
            x: pieCx,
            y: pieCy,
            radius: pieRadius,
            fill: '#e5e7eb',
            stroke: isToday(dateStr) ? CONFIG.colors.today : '#d1d5db',
            strokeWidth: isToday(dateStr) ? 2 : 1
        });
        layers.content.add(pieCircle);

        // AM arc (donut style)
        if (amRatio > 0) {
            const amArc = new Konva.Arc({
                x: pieCx,
                y: pieCy,
                innerRadius: innerRadius,
                outerRadius: pieRadius - 1,
                angle: 180 * amRatio,
                rotation: -90,
                fill: CONFIG.colors.am.filled,
                opacity: 0.9
            });
            layers.content.add(amArc);
        }

        // PM arc (donut style)
        if (pmRatio > 0) {
            const pmArc = new Konva.Arc({
                x: pieCx,
                y: pieCy,
                innerRadius: innerRadius,
                outerRadius: pieRadius - 1,
                angle: 180 * pmRatio,
                rotation: 90,
                fill: CONFIG.colors.pm.filled,
                opacity: 0.9
            });
            layers.content.add(pmArc);
        }

        // Center white circle for text background
        const centerCircle = new Konva.Circle({
            x: pieCx,
            y: pieCy,
            radius: innerRadius - 1,
            fill: 'white',
            stroke: '#e5e7eb',
            strokeWidth: 0.5
        });
        layers.content.add(centerCircle);

        // Day number text (centered in white circle)
        const textColor = dayOfWeek === 0 ? CONFIG.colors.weekend.sun :
            dayOfWeek === 6 ? CONFIG.colors.weekend.sat : '#374151';
        const dayText = new Konva.Text({
            x: pieCx - innerRadius,
            y: pieCy - CONFIG.font.sizeLarge / 2,
            width: innerRadius * 2,
            text: String(dayNumber),
            fontSize: CONFIG.font.sizeLarge,
            fontFamily: CONFIG.font.family,
            fontStyle: isToday(dateStr) ? 'bold' : 'normal',
            fill: isToday(dateStr) ? CONFIG.colors.today : textColor,
            align: 'center'
        });
        layers.content.add(dayText);

        // Total count badge (top right corner)
        if (total > 0) {
            const badge = new Konva.Label({
                x: x + width - 28,
                y: y + 4
            });
            badge.add(new Konva.Tag({
                fill: '#10b981',
                cornerRadius: 8
            }));
            badge.add(new Konva.Text({
                text: String(total),
                fontSize: CONFIG.font.sizeSmall,
                fontFamily: CONFIG.font.family,
                fill: 'white',
                padding: 3
            }));
            layers.content.add(badge);
        }

        // Interaction area
        const hitArea = new Konva.Rect({
            x: x,
            y: y,
            width: width,
            height: height,
            fill: 'transparent'
        });
        hitArea.on('mouseenter', function (e) {
            cellBg.fill(CONFIG.colors.hover);
            layers.grid.batchDraw();
            showTooltip(e.evt.clientX, e.evt.clientY,
                `<strong>${dateStr}</strong><br>午前: ${stats.am}件<br>午後: ${stats.pm}件<br>合計: ${total}件`);
        });
        hitArea.on('mouseleave', function () {
            cellBg.fill(isToday(dateStr) ? 'rgba(16, 185, 129, 0.1)' : 'white');
            layers.grid.batchDraw();
            hideTooltip();
        });
        hitArea.on('click', function () {
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateSelected', dateStr);
            }
        });
        layers.interaction.add(hitArea);
    }

    // ============================================================
    // Week Scheduler Renderer
    // ============================================================
    function renderWeekView() {
        const { stage, layers, currentDate, appointments, options } = state;
        const width = stage.width();
        const height = stage.height();

        layers.grid.destroyChildren();
        layers.content.destroyChildren();
        layers.interaction.destroyChildren();

        const { weekDays, startHour, endHour } = options;
        const hours = endHour - startHour;
        const timeColumnWidth = 60;
        const headerHeight = 50;
        const dayWidth = (width - timeColumnWidth) / weekDays;
        const hourHeight = (height - headerHeight) / hours;

        const startDate = getStartOfWeek(currentDate);

        // Header background
        const headerBg = new Konva.Rect({
            x: 0,
            y: 0,
            width: width,
            height: headerHeight,
            fill: '#f9fafb',
            stroke: CONFIG.colors.grid,
            strokeWidth: 1
        });
        layers.grid.add(headerBg);

        // Time column header
        const timeHeader = new Konva.Rect({
            x: 0,
            y: 0,
            width: timeColumnWidth,
            height: headerHeight,
            fill: '#f3f4f6',
            stroke: CONFIG.colors.grid,
            strokeWidth: 1
        });
        layers.grid.add(timeHeader);

        // Day headers
        const dayNames = ['日', '月', '火', '水', '木', '金', '土'];
        for (let i = 0; i < weekDays; i++) {
            const date = new Date(startDate);
            date.setDate(date.getDate() + i);
            const dayOfWeek = date.getDay();
            const dateStr = dateToString(date);
            const x = timeColumnWidth + i * dayWidth;

            // Header cell
            const headerCell = new Konva.Rect({
                x: x,
                y: 0,
                width: dayWidth,
                height: headerHeight,
                fill: isToday(date) ? 'rgba(16, 185, 129, 0.1)' : '#f9fafb',
                stroke: CONFIG.colors.grid,
                strokeWidth: 1
            });
            layers.grid.add(headerCell);

            // Day name
            const dayText = new Konva.Text({
                x: x,
                y: 8,
                width: dayWidth,
                text: dayNames[dayOfWeek],
                fontSize: CONFIG.font.sizeMedium,
                fontFamily: CONFIG.font.family,
                fill: dayOfWeek === 0 ? CONFIG.colors.weekend.sun :
                    dayOfWeek === 6 ? CONFIG.colors.weekend.sat : '#6b7280',
                align: 'center'
            });
            layers.grid.add(dayText);

            // Date number
            const dateText = new Konva.Text({
                x: x,
                y: 26,
                width: dayWidth,
                text: `${date.getMonth() + 1}/${date.getDate()}`,
                fontSize: CONFIG.font.sizeLarge,
                fontFamily: CONFIG.font.family,
                fontStyle: isToday(date) ? 'bold' : 'normal',
                fill: dayOfWeek === 0 ? CONFIG.colors.weekend.sun :
                    dayOfWeek === 6 ? CONFIG.colors.weekend.sat : '#374151',
                align: 'center'
            });
            layers.grid.add(dateText);
        }

        // Time slots and grid
        for (let h = 0; h < hours; h++) {
            const hour = startHour + h;
            const y = headerHeight + h * hourHeight;

            // Time label
            const timeLabel = new Konva.Text({
                x: 5,
                y: y + 5,
                text: `${String(hour).padStart(2, '0')}:00`,
                fontSize: CONFIG.font.sizeMedium,
                fontFamily: CONFIG.font.family,
                fill: '#6b7280'
            });
            layers.grid.add(timeLabel);

            // Hour row background
            const rowBg = new Konva.Rect({
                x: timeColumnWidth,
                y: y,
                width: width - timeColumnWidth,
                height: hourHeight,
                fill: h % 2 === 0 ? 'white' : '#fafafa',
                stroke: CONFIG.colors.grid,
                strokeWidth: 0.5
            });
            layers.grid.add(rowBg);

            // Day column separators and interaction areas
            for (let i = 0; i < weekDays; i++) {
                const x = timeColumnWidth + i * dayWidth;
                const date = new Date(startDate);
                date.setDate(date.getDate() + i);
                const dateStr = dateToString(date);

                // Column separator
                const colSep = new Konva.Line({
                    points: [x, y, x, y + hourHeight],
                    stroke: CONFIG.colors.grid,
                    strokeWidth: 1
                });
                layers.grid.add(colSep);

                // Interaction area
                const hitArea = new Konva.Rect({
                    x: x,
                    y: y,
                    width: dayWidth,
                    height: hourHeight,
                    fill: 'transparent'
                });
                hitArea.on('mouseenter', function () {
                    hitArea.fill(CONFIG.colors.hover);
                    layers.interaction.batchDraw();
                });
                hitArea.on('mouseleave', function () {
                    hitArea.fill('transparent');
                    layers.interaction.batchDraw();
                });
                hitArea.on('click', function () {
                    if (state.dotNetRef) {
                        state.dotNetRef.invokeMethodAsync('OnCreateRequested', dateStr, `${String(hour).padStart(2, '0')}:00`);
                    }
                });
                layers.interaction.add(hitArea);
            }
        }

        // Render appointments
        renderAppointments(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate);

        layers.grid.batchDraw();
        layers.content.batchDraw();
        layers.interaction.batchDraw();
    }

    function renderAppointments(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate) {
        const { layers, appointments, options } = state;
        const { startHour, weekDays } = options;

        appointments.forEach(appt => {
            const apptDate = parseDate(appt.date);
            const dayIndex = Math.floor((apptDate - startDate) / (1000 * 60 * 60 * 24));

            if (dayIndex < 0 || dayIndex >= weekDays) return;

            const startParts = appt.startTime.split(':');
            const endParts = appt.endTime.split(':');
            const startHourNum = parseInt(startParts[0]);
            const startMin = parseInt(startParts[1]) || 0;
            const endHourNum = parseInt(endParts[0]);
            const endMin = parseInt(endParts[1]) || 0;

            const startOffset = (startHourNum - startHour) + (startMin / 60);
            const duration = (endHourNum - startHourNum) + ((endMin - startMin) / 60);

            const x = timeColumnWidth + dayIndex * dayWidth + 2;
            const y = headerHeight + startOffset * hourHeight;
            const w = dayWidth - 4;
            const h = duration * hourHeight - 2;

            const statusColor = CONFIG.colors.status[appt.status] || CONFIG.colors.status[0];

            // Appointment block
            const block = new Konva.Rect({
                x: x,
                y: y,
                width: w,
                height: h,
                fill: statusColor,
                opacity: 0.9,
                cornerRadius: 4,
                shadowColor: 'black',
                shadowBlur: 2,
                shadowOpacity: 0.2,
                shadowOffsetY: 1
            });
            layers.content.add(block);

            // Patient name
            const nameText = new Konva.Text({
                x: x + 4,
                y: y + 4,
                width: w - 8,
                text: appt.patientName || '未設定',
                fontSize: CONFIG.font.sizeMedium,
                fontFamily: CONFIG.font.family,
                fontStyle: 'bold',
                fill: '#1f2937',
                ellipsis: true
            });
            layers.content.add(nameText);

            // Organization name
            if (appt.orgName && h > 35) {
                const orgText = new Konva.Text({
                    x: x + 4,
                    y: y + 20,
                    width: w - 8,
                    text: appt.orgName,
                    fontSize: CONFIG.font.sizeSmall,
                    fontFamily: CONFIG.font.family,
                    fill: '#4b5563',
                    ellipsis: true
                });
                layers.content.add(orgText);
            }

            // Appointment interaction
            const apptHitArea = new Konva.Rect({
                x: x,
                y: y,
                width: w,
                height: h,
                fill: 'transparent'
            });
            apptHitArea.on('mouseenter', function (e) {
                block.opacity(1);
                block.shadowBlur(4);
                layers.content.batchDraw();
                showTooltip(e.evt.clientX, e.evt.clientY,
                    `<strong>${appt.patientName || '未設定'}</strong><br>` +
                    `${appt.orgName || ''}<br>` +
                    `${appt.startTime} - ${appt.endTime}<br>` +
                    `ステータス: ${getStatusText(appt.status)}`);
            });
            apptHitArea.on('mouseleave', function () {
                block.opacity(0.9);
                block.shadowBlur(2);
                layers.content.batchDraw();
                hideTooltip();
            });
            apptHitArea.on('click', function () {
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnAppointmentClicked', appt.id);
                }
            });
            layers.interaction.add(apptHitArea);
        });
    }

    function getStatusText(status) {
        const statusTexts = {
            0: '予約',
            1: '待機中',
            2: '来院済み',
            3: 'キャンセル'
        };
        return statusTexts[status] || '不明';
    }

    // ============================================================
    // Public API
    // ============================================================
    return {
        /**
         * Initialize the calendar canvas
         * @param {string} containerId - DOM container ID
         * @param {object} data - Initial data { appointments, dayStats }
         * @param {object} options - Configuration options
         * @param {object} dotNetRef - .NET object reference for callbacks
         */
        init: function (containerId, data, options, dotNetRef) {
            state.containerId = containerId;
            state.dotNetRef = dotNetRef;
            state.options = { ...state.options, ...options };

            const container = document.getElementById(containerId);
            if (!container) {
                console.error('MedockCalendar: Container not found:', containerId);
                return;
            }

            // Create stage
            state.stage = new Konva.Stage({
                container: containerId,
                width: container.clientWidth,
                height: container.clientHeight || 600
            });

            createLayers();
            createTooltip();

            // Set initial data
            if (data) {
                this.updateData(data);
            }

            // Handle resize
            const resizeObserver = new ResizeObserver(entries => {
                for (let entry of entries) {
                    state.stage.width(entry.contentRect.width);
                    state.stage.height(entry.contentRect.height || 600);
                    this.render();
                }
            });
            resizeObserver.observe(container);

            // Initial render
            this.render();
        },

        /**
         * Update calendar data
         * @param {object} data - { appointments: [], dayStats: {} }
         */
        updateData: function (data) {
            if (data.appointments) {
                state.appointments = data.appointments;
            }

            if (data.dayStats) {
                state.dayStats = new Map(Object.entries(data.dayStats));
            }

            this.render();
        },

        /**
         * Change the current view
         * @param {string} viewType - 'year', 'month', 'week'
         * @param {string} dateStr - Optional date string 'YYYY-MM-DD'
         */
        changeView: function (viewType, dateStr) {
            state.currentView = viewType;
            if (dateStr) {
                state.currentDate = parseDate(dateStr);
            }
            this.render();
        },

        /**
         * Set week scheduler options
         * @param {object} options - { weekDays: number, showSlots: boolean }
         */
        setOptions: function (options) {
            state.options = { ...state.options, ...options };
            this.render();
        },

        /**
         * Navigate to a specific date
         * @param {string} dateStr - Date string 'YYYY-MM-DD'
         */
        navigateTo: function (dateStr) {
            state.currentDate = parseDate(dateStr);
            this.render();
        },

        /**
         * Render the current view
         */
        render: function () {
            if (!state.stage) return;

            // Ensure stage size matches container
            const container = document.getElementById(state.containerId);
            if (container) {
                const width = container.clientWidth;
                const height = container.clientHeight || 600;
                if (state.stage.width() !== width || state.stage.height() !== height) {
                    state.stage.width(width);
                    state.stage.height(height);
                }
            }

            switch (state.currentView) {
                case 'year':
                    renderYearView();
                    break;
                case 'month':
                    renderMonthView();
                    break;
                case 'week':
                    renderWeekView();
                    break;
            }
        },

        /**
         * Destroy the calendar instance
         */
        destroy: function () {
            if (state.stage) {
                state.stage.destroy();
            }
            if (state.tooltip && state.tooltip.parentNode) {
                state.tooltip.parentNode.removeChild(state.tooltip);
            }
            state = {
                stage: null,
                layers: {},
                dotNetRef: null,
                containerId: null,
                currentView: 'month',
                currentDate: new Date(),
                appointments: [],
                dayStats: new Map(),
                options: {
                    weekDays: 7,
                    showSlots: true,
                    startHour: 8,
                    endHour: 18
                },
                tooltip: null,
                hoveredElement: null
            };
        }
    };
})();

