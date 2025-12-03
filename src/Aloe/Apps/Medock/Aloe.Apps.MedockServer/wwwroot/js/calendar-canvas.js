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
            // 時間帯枠用の色（空き→青、満杯→赤）
            slot: {
                empty: '#3b82f6',      // 空き（青）
                full: '#ef4444',       // 満杯（赤）
                partial: '#fbbf24',    // 一部埋まり（黄）
                background: '#e5e7eb'  // 背景（グレー）
            },
            status: {
                0: '#fbbf24', // Reserved (warning)
                1: '#60a5fa', // Waiting (info)
                2: '#34d399', // Visited (success)
                3: '#f87171'  // Cancelled (error)
            },
            grid: '#d1d5db',
            today: '#10b981',
            weekend: { sun: '#ef4444', sat: '#3b82f6' },
            hover: 'rgba(16, 185, 129, 0.1)',
            grayout: 'rgba(128, 128, 128, 0.5)'
        },
        font: {
            family: '"M PLUS 1 Code", system-ui, -apple-system, sans-serif',
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
        dayStats: new Map(), // date string -> { am: count, pm: count, total: count, slots: [] }
        holidays: new Map(), // date string -> holiday name
        options: {
            weekDays: 7, // 1, 3, 7, 14, 31
            showSlots: true,
            startHour: 8,
            endHour: 18
        },
        tooltip: null,
        hoveredElement: null,
        // インタラクション用の追加状態
        selectedDate: null,           // 選択中の日付
        selectedDateRange: null,      // 範囲選択 { start: string, end: string }
        lastClickTime: 0,             // ダブルクリック検出用
        lastClickDate: null,          // ダブルクリック検出用
        isDragging: false,            // ドラッグ中フラグ
        dragStartDate: null           // ドラッグ開始日
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
        
        // Convert client coordinates to container-relative coordinates
        const container = document.getElementById(state.containerId);
        if (container) {
            const rect = container.getBoundingClientRect();
            x = x - rect.left;
            y = y - rect.top;
        }
        
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

        // Month header (クリック可能)
        const headerBg = new Konva.Rect({
            x: x,
            y: y - 2,
            width: width,
            height: 22,
            fill: 'transparent',
            cornerRadius: 4
        });
        layers.grid.add(headerBg);

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

        // 月ヘッダーのインタラクション
        const headerHitArea = new Konva.Rect({
            x: x,
            y: y - 2,
            width: width,
            height: 22,
            fill: 'transparent'
        });
        headerHitArea.on('mouseenter', function () {
            headerBg.fill('rgba(59, 130, 246, 0.1)');
            document.body.style.cursor = 'pointer';
            layers.grid.batchDraw();
        });
        headerHitArea.on('mouseleave', function () {
            headerBg.fill('transparent');
            document.body.style.cursor = 'default';
            layers.grid.batchDraw();
        });
        headerHitArea.on('click', function () {
            // 月クリック → 月間表示に切り替え
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnMonthClicked', year, month + 1);
            }
        });
        layers.interaction.add(headerHitArea);

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

        // Draw grid lines and weekend backgrounds
        // サブピクセルレンダリング問題を回避するために座標を0.5pxオフセット
        function snapToPixel(val) {
            return Math.floor(val) + 0.5;
        }
        
        for (let row = 0; row <= rows; row++) {
            for (let col = 0; col < 7; col++) {
                const cellLeft = x + col * cellWidth;
                const cellTop = gridTop + (row + 1) * cellHeight;
                
                // Weekend background color
                if (row < rows) {
                    let bgColor = null;
                    if (col === 0) bgColor = 'rgba(239, 68, 68, 0.08)'; // Sunday - light red
                    else if (col === 6) bgColor = 'rgba(59, 130, 246, 0.08)'; // Saturday - light blue
                    
                    if (bgColor) {
                        const weekendBg = new Konva.Rect({
                            x: cellLeft,
                            y: cellTop,
                            width: cellWidth,
                            height: cellHeight,
                            fill: bgColor
                        });
                        layers.background.add(weekendBg);
                    }
                }
                
                // Vertical grid lines (曜日ヘッダーから最下段まで)
                if (col > 0) {
                    const vLine = new Konva.Line({
                        points: [snapToPixel(cellLeft), snapToPixel(gridTop), snapToPixel(cellLeft), snapToPixel(gridTop + (rows + 1) * cellHeight)],
                        stroke: '#e5e7eb',
                        strokeWidth: 1
                    });
                    layers.grid.add(vLine);
                }
            }
            
            // Horizontal grid lines
            if (row > 0) {
                const hLine = new Konva.Line({
                    points: [snapToPixel(x), snapToPixel(gridTop + (row + 1) * cellHeight), snapToPixel(x + width), snapToPixel(gridTop + (row + 1) * cellHeight)],
                    stroke: '#e5e7eb',
                    strokeWidth: 1
                });
                layers.grid.add(hLine);
            }
        }
        
        // 曜日ヘッダーと日付セルの境界線を追加
        const headerBottomLine = new Konva.Line({
            points: [snapToPixel(x), snapToPixel(gridTop + cellHeight), snapToPixel(x + width), snapToPixel(gridTop + cellHeight)],
            stroke: '#e5e7eb',
            strokeWidth: 1
        });
        layers.grid.add(headerBottomLine);

        // Day cells with pie charts
        let day = 1;
        for (let row = 0; row < rows; row++) {
            for (let col = 0; col < 7; col++) {
                const cellIndex = row * 7 + col;
                if (cellIndex < startDayOfWeek || day > daysInMonth) continue;

                const cellLeft = x + col * cellWidth;
                const cellTop = gridTop + (row + 1) * cellHeight;
                const cellX = cellLeft + cellWidth / 2;
                const cellY = cellTop + cellHeight / 2;
                const radius = Math.min(cellWidth, cellHeight) / 2 - 2;

                const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
                const isHoliday = state.holidays.has(dateStr);
                
                // Holiday background (light red, same as Sunday)
                if (isHoliday && !isToday(dateStr)) {
                    const holidayBg = new Konva.Rect({
                        x: cellLeft + 1,
                        y: cellTop + 1,
                        width: cellWidth - 2,
                        height: cellHeight - 2,
                        fill: 'rgba(239, 68, 68, 0.12)',
                        cornerRadius: 2
                    });
                    layers.grid.add(holidayBg);
                }
                
                // Today's cell background (bright green rectangle)
                if (isToday(dateStr)) {
                    const todayBg = new Konva.Rect({
                        x: cellLeft + 1,
                        y: cellTop + 1,
                        width: cellWidth - 2,
                        height: cellHeight - 2,
                        fill: 'rgba(16, 185, 129, 0.3)',
                        cornerRadius: 2
                    });
                    layers.grid.add(todayBg);
                }
                
                const stats = dayStats.get(dateStr) || { am: 0, pm: 0, amMax: 10, pmMax: 10 };
                const amRatio = stats.amMax > 0 ? stats.am / stats.amMax : 0;
                const pmRatio = stats.pmMax > 0 ? stats.pm / stats.pmMax : 0;

                renderDayPieChart(cellX, cellY, radius, amRatio, pmRatio, dateStr, day, isHoliday);

                day++;
            }
        }
    }

    /**
     * 時間帯枠の充足率から色を取得
     * 空き→青、一部→黄、満杯→赤
     */
    function getSlotColor(ratio) {
        if (ratio >= 1.0) {
            return CONFIG.colors.slot.full; // 満杯 - 赤
        } else if (ratio >= 0.7) {
            // 70%以上 - 赤と黄の中間
            return '#f97316'; // orange
        } else if (ratio >= 0.3) {
            return CONFIG.colors.slot.partial; // 一部埋まり - 黄
        } else if (ratio > 0) {
            // 30%未満 - 黄と青の中間
            return '#22d3ee'; // cyan
        } else {
            return CONFIG.colors.slot.empty; // 空き - 青
        }
    }

    function renderDayPieChart(cx, cy, radius, amRatio, pmRatio, dateStr, dayNumber, isHoliday = false) {
        const { layers } = state;
        const innerRadius = radius * 0.4; // ドーナツの内側半径（太い色付き部分）
        
        // 時間帯枠データを取得
        const stats = state.dayStats.get(dateStr);
        const slots = stats?.slots || null;
        const isDateGrayed = stats?.isGrayedOut || false;
        
        // Background circle (outer ring)
        const bgCircle = new Konva.Circle({
            x: cx,
            y: cy,
            radius: radius,
            fill: CONFIG.colors.slot.background
        });
        layers.content.add(bgCircle);

        if (slots && slots.length > 0) {
            // 時間帯枠ベースの表示（新方式）
            const slotCount = slots.length;
            const anglePerSlot = 360 / slotCount;
            
            slots.forEach((slot, index) => {
                const ratio = slot.max > 0 ? slot.count / slot.max : 0;
                const isSlotGrayed = slot.isGrayedOut || isDateGrayed;
                // グレーアウト時は彩度を落とす
                const slotColor = isSlotGrayed ? '#9ca3af' : getSlotColor(ratio);
                const startAngle = -90 + (index * anglePerSlot);
                
                const slotArc = new Konva.Arc({
                    x: cx,
                    y: cy,
                    innerRadius: innerRadius,
                    outerRadius: radius,
                    angle: anglePerSlot - 1, // 1度の隙間
                    rotation: startAngle,
                    fill: slotColor,
                    opacity: isSlotGrayed ? 0.4 : 1
                });
                layers.content.add(slotArc);
            });
        } else {
            // フォールバック: AM/PM方式（旧方式）
            if (amRatio > 0) {
                const amColor = getSlotColor(amRatio);
                const amArc = new Konva.Arc({
                    x: cx,
                    y: cy,
                    innerRadius: innerRadius,
                    outerRadius: radius,
                    angle: 180 * Math.min(amRatio, 1),
                    rotation: -90,
                    fill: amColor
                });
                layers.content.add(amArc);
            }

            if (pmRatio > 0) {
                const pmColor = getSlotColor(pmRatio);
                const pmArc = new Konva.Arc({
                    x: cx,
                    y: cy,
                    innerRadius: innerRadius,
                    outerRadius: radius,
                    angle: 180 * Math.min(pmRatio, 1),
                    rotation: 90,
                    fill: pmColor
                });
                layers.content.add(pmArc);
            }
        }

        // Center white circle for text background
        const centerCircle = new Konva.Circle({
            x: cx,
            y: cy,
            radius: innerRadius,
            fill: isDateGrayed ? '#e5e7eb' : 'white',
            opacity: isDateGrayed ? 0.6 : 1
        });
        layers.content.add(centerCircle);

        // Day number text (centered in white circle) - nowrap対応
        const dayOfWeek = new Date(dateStr).getDay();
        // 祝日と日曜日は赤色で表示（グレーアウト時は灰色）
        let textColor;
        if (isDateGrayed) {
            textColor = '#9ca3af'; // グレー
        } else if (isHoliday || dayOfWeek === 0) {
            textColor = CONFIG.colors.weekend.sun;
        } else if (dayOfWeek === 6) {
            textColor = CONFIG.colors.weekend.sat;
        } else {
            textColor = '#374151';
        }
        const text = new Konva.Text({
            x: cx - innerRadius,
            y: cy - CONFIG.font.sizeSmall / 2,
            width: innerRadius * 2,
            text: String(dayNumber),
            fontSize: CONFIG.font.sizeSmall,
            fontFamily: CONFIG.font.family,
            fontStyle: 'normal',
            fill: textColor,
            align: 'center',
            wrap: 'none' // 改行防止
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
            let tooltipContent = `<strong>${dateStr}</strong><br>`;
            if (slots && slots.length > 0) {
                const totalCount = slots.reduce((sum, s) => sum + s.count, 0);
                const totalMax = slots.reduce((sum, s) => sum + s.max, 0);
                tooltipContent += `予約: ${totalCount}/${totalMax}件<br>`;
                tooltipContent += `<small>`;
                slots.forEach(s => {
                    const emoji = s.count >= s.max ? '🔴' : s.count > 0 ? '🟡' : '🔵';
                    tooltipContent += `${s.time}: ${emoji} ${s.count}/${s.max}<br>`;
                });
                tooltipContent += `</small>`;
            } else {
                const st = state.dayStats.get(dateStr) || { am: 0, pm: 0 };
                tooltipContent += `午前: ${st.am}件<br>午後: ${st.pm}件`;
            }
            showTooltip(e.evt.clientX, e.evt.clientY, tooltipContent);
            bgCircle.stroke(CONFIG.colors.today);
            bgCircle.strokeWidth(2);
            layers.content.batchDraw();
        });
        hitArea.on('mouseleave', function () {
            hideTooltip();
            // ドラッグ中でなければストロークをリセット
            if (!state.isDragging) {
                const isSelected = state.selectedDate === dateStr || 
                    (state.selectedDateRange && isDateInRange(dateStr, state.selectedDateRange));
                bgCircle.stroke(isSelected ? '#3b82f6' : isToday(dateStr) ? CONFIG.colors.today : null);
                bgCircle.strokeWidth(isSelected || isToday(dateStr) ? 2 : 0);
            }
            layers.content.batchDraw();
        });
        
        // ダブルクリック検出用のクリックハンドラ
        hitArea.on('click', function () {
            const now = Date.now();
            const isDoubleClick = (now - state.lastClickTime < 300) && (state.lastClickDate === dateStr);
            
            if (isDoubleClick) {
                // ダブルクリック → その日のスケジュール表示
                state.lastClickTime = 0;
                state.lastClickDate = null;
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnDateDoubleClicked', dateStr);
                }
            } else {
                // シングルクリック → 日付選択
                state.lastClickTime = now;
                state.lastClickDate = dateStr;
                state.selectedDate = dateStr;
                state.selectedDateRange = null;
                
                // 選択状態を視覚的に反映（再描画）
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', dateStr);
                }
            }
        });
        
        // ドラッグによる範囲選択
        hitArea.on('mousedown', function (e) {
            state.isDragging = true;
            state.dragStartDate = dateStr;
            state.selectedDateRange = { start: dateStr, end: dateStr };
        });
        
        hitArea.on('mousemove', function (e) {
            if (state.isDragging && state.dragStartDate) {
                state.selectedDateRange = {
                    start: state.dragStartDate,
                    end: dateStr
                };
                // ドラッグ中の視覚的フィードバック
                bgCircle.stroke('#3b82f6');
                bgCircle.strokeWidth(2);
                layers.content.batchDraw();
            }
        });
        
        layers.interaction.add(hitArea);
    }

    /**
     * 日付が範囲内かどうかを判定
     */
    function isDateInRange(dateStr, range) {
        if (!range || !range.start || !range.end) return false;
        const date = parseDate(dateStr);
        const start = parseDate(range.start);
        const end = parseDate(range.end);
        const minDate = start < end ? start : end;
        const maxDate = start < end ? end : start;
        return date >= minDate && date <= maxDate;
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
        const isHoliday = state.holidays.has(dateStr);

        // Determine background color (priority: today > holiday > weekend > normal)
        let bgColor = 'white';
        if (isToday(dateStr)) {
            bgColor = 'rgba(16, 185, 129, 0.25)';
        } else if (isHoliday) {
            bgColor = 'rgba(239, 68, 68, 0.12)';
        } else if (dayOfWeek === 0) {
            bgColor = 'rgba(239, 68, 68, 0.08)';
        } else if (dayOfWeek === 6) {
            bgColor = 'rgba(59, 130, 246, 0.08)';
        }

        // Cell background
        const cellBg = new Konva.Rect({
            x: x,
            y: y,
            width: width,
            height: height,
            fill: bgColor,
            stroke: isToday(dateStr) ? CONFIG.colors.today : CONFIG.colors.grid,
            strokeWidth: isToday(dateStr) ? 2 : 1
        });
        layers.grid.add(cellBg);

        // AM/PM Donut chart centered in cell
        const pieRadius = Math.min(width, height) * 0.35;
        const innerRadius = pieRadius * 0.4; // 太い色付き部分
        const pieCx = x + width / 2;
        const pieCy = y + height / 2;

        // Background circle (outer ring) - no gap
        const pieCircle = new Konva.Circle({
            x: pieCx,
            y: pieCy,
            radius: pieRadius,
            fill: '#d1d5db'
        });
        layers.content.add(pieCircle);

        // AM arc (donut style) - no gap
        if (amRatio > 0) {
            const amArc = new Konva.Arc({
                x: pieCx,
                y: pieCy,
                innerRadius: innerRadius,
                outerRadius: pieRadius,
                angle: 180 * amRatio,
                rotation: -90,
                fill: CONFIG.colors.am.filled
            });
            layers.content.add(amArc);
        }

        // PM arc (donut style) - no gap
        if (pmRatio > 0) {
            const pmArc = new Konva.Arc({
                x: pieCx,
                y: pieCy,
                innerRadius: innerRadius,
                outerRadius: pieRadius,
                angle: 180 * pmRatio,
                rotation: 90,
                fill: CONFIG.colors.pm.filled
            });
            layers.content.add(pmArc);
        }

        // Center white circle for text background
        const centerCircle = new Konva.Circle({
            x: pieCx,
            y: pieCy,
            radius: innerRadius,
            fill: 'white'
        });
        layers.content.add(centerCircle);

        // Day number text (centered in white circle)
        // 祝日と日曜日は赤色で表示
        const textColor = (isHoliday || dayOfWeek === 0) ? CONFIG.colors.weekend.sun :
            dayOfWeek === 6 ? CONFIG.colors.weekend.sat : '#374151';
        const dayText = new Konva.Text({
            x: pieCx - innerRadius,
            y: pieCy - CONFIG.font.sizeLarge / 2,
            width: innerRadius * 2,
            text: String(dayNumber),
            fontSize: CONFIG.font.sizeLarge,
            fontFamily: CONFIG.font.family,
            fontStyle: 'normal',
            fill: textColor,
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
            // 元の背景色に戻す（優先順位: today > holiday > weekend > normal）
            cellBg.fill(bgColor);
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
        const { startHour, weekDays, showSlots } = options;

        if (showSlots) {
            // スロット表示モード: タイムスロット枠を表示
            renderSlotMode(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate);
        } else {
            // 詳細表示モード: アバター表示
            renderDetailMode(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate);
        }
    }

    function renderSlotMode(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate) {
        const { layers, appointments, options } = state;
        const { startHour, weekDays } = options;

        // 30分単位のスロットを表示
        const slotMinutes = 30;
        const slotHeight = hourHeight / 2;
        const maxPerSlot = 4; // 1スロットあたり最大4人

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

    function renderDetailMode(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate) {
        const { layers, appointments, options } = state;
        const { startHour, weekDays } = options;

        // 苗字の最初の文字を取得
        function getInitial(name) {
            if (!name) return '?';
            // スペースで分割して苗字を取得
            const parts = name.split(/[\s　]/);
            return parts[0].charAt(0);
        }

        // 色のパレット（アバターの色）
        const avatarColors = [
            '#3b82f6', // blue
            '#10b981', // green
            '#f59e0b', // amber
            '#ef4444', // red
            '#8b5cf6', // purple
            '#ec4899', // pink
            '#06b6d4', // cyan
            '#f97316', // orange
        ];

        // スロットごとに予約をグループ化（30分単位）
        const slotMap = new Map(); // key: "dayIndex-hour-half" -> appointments[]

        appointments.forEach(appt => {
            const apptDate = parseDate(appt.date);
            const dayIndex = Math.floor((apptDate - startDate) / (1000 * 60 * 60 * 24));

            if (dayIndex < 0 || dayIndex >= weekDays) return;

            const startParts = appt.startTime.split(':');
            const hourNum = parseInt(startParts[0]);
            const minNum = parseInt(startParts[1]) || 0;
            const half = minNum >= 30 ? 1 : 0;
            const slotKey = `${dayIndex}-${hourNum}-${half}`;

            if (!slotMap.has(slotKey)) {
                slotMap.set(slotKey, []);
            }
            slotMap.get(slotKey).push(appt);
        });

        // 各スロットにアバターを描画
        slotMap.forEach((slotAppts, slotKey) => {
            const [dayIndex, hourNum, half] = slotKey.split('-').map(Number);
            const slotHeight = hourHeight / 2;
            const avatarSize = Math.min(28, slotHeight - 4, (dayWidth - 8) / Math.min(slotAppts.length, 4) - 4);
            
            const baseX = timeColumnWidth + dayIndex * dayWidth + 4;
            const baseY = headerHeight + (hourNum - startHour) * hourHeight + half * slotHeight + (slotHeight - avatarSize) / 2;

            slotAppts.forEach((appt, idx) => {
                if (idx >= 4) return; // 1スロット最大4人まで表示
                
                const avatarX = baseX + idx * (avatarSize + 4);
                const avatarY = baseY;
                const initial = getInitial(appt.patientName);
                const colorIdx = appt.patientName ? appt.patientName.charCodeAt(0) % avatarColors.length : 0;
                const avatarColor = avatarColors[colorIdx];

                // アバター円
                const avatar = new Konva.Circle({
                    x: avatarX + avatarSize / 2,
                    y: avatarY + avatarSize / 2,
                    radius: avatarSize / 2,
                    fill: avatarColor,
                    stroke: 'white',
                    strokeWidth: 2,
                    shadowColor: 'rgba(0,0,0,0.3)',
                    shadowBlur: 3,
                    shadowOffsetY: 1,
                    draggable: true
                });
                avatar.id(appt.id);

                // イニシャル文字
                const initialText = new Konva.Text({
                    x: avatarX,
                    y: avatarY + avatarSize / 2 - 7,
                    width: avatarSize,
                    text: initial,
                    fontSize: 14,
                    fontFamily: CONFIG.font.family,
                    fontStyle: 'bold',
                    fill: 'white',
                    align: 'center',
                    listening: false
                });

                // グループにまとめてドラッグ可能に
                const group = new Konva.Group({
                    x: 0,
                    y: 0,
                    draggable: true
                });
                group.add(avatar);
                group.add(initialText);
                group.id(appt.id);

                // ホバー時のハイライト
                group.on('mouseenter', function (e) {
                    avatar.stroke('#fbbf24');
                    avatar.strokeWidth(3);
                    avatar.shadowBlur(6);
                    layers.content.batchDraw();
                    document.body.style.cursor = 'pointer';
                    showTooltip(e.evt.clientX, e.evt.clientY,
                        `<strong>${appt.patientName || '未設定'}</strong><br>` +
                        `${appt.orgName || ''}<br>` +
                        `${appt.startTime} - ${appt.endTime}<br>` +
                        `ステータス: ${getStatusText(appt.status)}`);
                });

                group.on('mouseleave', function () {
                    avatar.stroke('white');
                    avatar.strokeWidth(2);
                    avatar.shadowBlur(3);
                    layers.content.batchDraw();
                    document.body.style.cursor = 'default';
                    hideTooltip();
                });

                // クリックで選択
                group.on('click', function () {
                    if (state.dotNetRef) {
                        state.dotNetRef.invokeMethodAsync('OnAppointmentClicked', appt.id);
                    }
                });

                // ドラッグ開始時
                group.on('dragstart', function () {
                    avatar.shadowBlur(8);
                    avatar.shadowColor('rgba(0,0,0,0.5)');
                    group.moveToTop();
                    hideTooltip();
                });

                // ドラッグ終了時（ドロップ位置から新しい日時を計算）
                group.on('dragend', function () {
                    avatar.shadowBlur(3);
                    avatar.shadowColor('rgba(0,0,0,0.3)');
                    
                    const pos = group.position();
                    const dropX = avatarX + avatarSize / 2 + pos.x;
                    const dropY = avatarY + avatarSize / 2 + pos.y;
                    
                    // ドロップ位置から日付と時間を計算
                    const newDayIndex = Math.floor((dropX - timeColumnWidth) / dayWidth);
                    const newHourOffset = (dropY - headerHeight) / hourHeight;
                    const newHour = Math.floor(startHour + newHourOffset);
                    const newMin = (newHourOffset % 1) >= 0.5 ? 30 : 0;
                    
                    if (newDayIndex >= 0 && newDayIndex < weekDays && newHour >= startHour && newHour < options.endHour) {
                        const newDate = new Date(startDate);
                        newDate.setDate(newDate.getDate() + newDayIndex);
                        const newDateStr = dateToString(newDate);
                        const newTimeStr = `${String(newHour).padStart(2, '0')}:${String(newMin).padStart(2, '0')}`;
                        
                        // ドラッグ＆ドロップ完了をサーバーに通知
                        if (state.dotNetRef) {
                            state.dotNetRef.invokeMethodAsync('OnAppointmentMoved', appt.id, newDateStr, newTimeStr);
                        }
                    }
                    
                    // 元の位置に戻す（サーバー側で更新後にリフレッシュされる）
                    group.position({ x: 0, y: 0 });
                    layers.content.batchDraw();
                });

                layers.content.add(group);
            });
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

            // ドラッグ終了イベント（mouseup）をステージに追加
            state.stage.on('mouseup', function () {
                if (state.isDragging && state.selectedDateRange) {
                    const range = state.selectedDateRange;
                    // 範囲が有効な場合のみコールバック
                    if (range.start !== range.end && state.dotNetRef) {
                        // 日付の順序を正規化
                        const start = parseDate(range.start);
                        const end = parseDate(range.end);
                        const normalizedRange = start <= end 
                            ? { start: range.start, end: range.end }
                            : { start: range.end, end: range.start };
                        state.dotNetRef.invokeMethodAsync('OnDateRangeSelected', normalizedRange.start, normalizedRange.end);
                    }
                }
                state.isDragging = false;
                state.dragStartDate = null;
            });

            // コンテナ外でのmouseupも処理
            document.addEventListener('mouseup', function () {
                if (state.isDragging) {
                    state.isDragging = false;
                    state.dragStartDate = null;
                }
            });

            // Initial render
            this.render();
        },

        /**
         * Update calendar data
         * @param {object} data - { appointments: [], dayStats: {}, holidays: {} }
         */
        updateData: function (data) {
            if (data.appointments) {
                state.appointments = data.appointments;
            }

            if (data.dayStats) {
                state.dayStats = new Map(Object.entries(data.dayStats));
            }

            if (data.holidays) {
                state.holidays = new Map(Object.entries(data.holidays));
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
                holidays: new Map(),
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

