// Dot wave behind the product grid (lightweight + theme friendly)
(function () {
    const reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const canvas = document.getElementById('dotWaveGrid');
    if (!canvas || reduce) return;

    const ctx = canvas.getContext('2d', { alpha: true });
    let dpr = Math.max(1, window.devicePixelRatio || 1);
    let width = 0, height = 0;
    let cols = 0, rows = 0;
    let spacing = 34;
    let t = 0;
    const speed = 0.015;
    const amp = 12, amp2 = 7;

    // Subtle EasyGames colours
    const c1 = 'rgba(91,140,255,0.55)';   // blue
    const c2 = 'rgba(129,79,247,0.55)';   // purple

    function resize() {
        const rect = canvas.getBoundingClientRect();
        width = Math.max(1, Math.floor(rect.width));
        height = Math.max(1, Math.floor(rect.height));

        spacing = width >= 1200 ? 34 : width >= 768 ? 32 : 28;

        cols = Math.ceil(width / spacing) + 2;
        rows = Math.ceil(height / spacing) + 2;

        canvas.width = Math.floor(width * dpr);
        canvas.height = Math.floor(height * dpr);
        ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    }

    function draw() {
        ctx.clearRect(0, 0, width, height);

        const xOffset = ((width % spacing) / 2);
        const yOffset = ((height % spacing) / 2);

        for (let j = 0; j < rows; j++) {
            for (let i = 0; i < cols; i++) {
                const x = i * spacing + xOffset;
                const baseY = j * spacing + yOffset;

                const y =
                    baseY +
                    Math.sin((i * 0.55) + t * 2.0) * amp +
                    Math.sin((j * 0.45) - t * 1.5) * amp2;

                ctx.fillStyle = (i % 2 === 0) ? c1 : c2;
                const r = 1.4 + ((y - baseY) / (amp + amp2)) * 0.6; // slight size change
                ctx.beginPath();
                ctx.arc(x, y, Math.max(1, r), 0, Math.PI * 2);
                ctx.fill();
            }
        }
    }

    let raf;
    function loop() { t += speed; draw(); raf = requestAnimationFrame(loop); }

    document.addEventListener('visibilitychange', () => {
        if (document.hidden) cancelAnimationFrame(raf); else raf = requestAnimationFrame(loop);
    });
    window.addEventListener('resize', () => { dpr = Math.max(1, window.devicePixelRatio || 1); resize(); });

    resize();
    raf = requestAnimationFrame(loop);
})();
