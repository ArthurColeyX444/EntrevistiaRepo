(function () {
  function backgroundAnimation() {
    var width, height, canvas, ctx, points, target, animateHeader = true;

    initHeader();
    initAnimation();
    addListeners();

    function initHeader() {
      width = window.innerWidth;
      height = window.innerHeight;
      target = { x: width / 2, y: height / 2 };

      canvas = document.getElementById('bg-animation');
      canvas.width = width * (window.devicePixelRatio || 1);
      canvas.height = height * (window.devicePixelRatio || 1);
      canvas.style.width = width + 'px';
      canvas.style.height = height + 'px';
      ctx = canvas.getContext('2d');
      // scale for high-DPI
      var dpr = window.devicePixelRatio || 1;
      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

      // create points
      points = [];
      var xStep = Math.ceil(width / 10);
      var yStep = Math.ceil(height / 10);
      for (var x = 0; x < width; x = x + xStep) {
        for (var y = 0; y < height; y = y + yStep) {
          var px = x + Math.random() * xStep;
          var py = y + Math.random() * yStep;
          var p = { x: px, originX: px, y: py, originY: py, active: 0 };
          points.push(p);
        }
      }

      // for each point find the 5 closest points
      for (var i = 0; i < points.length; i++) {
        var closest = [];
        var p1 = points[i];
        for (var j = 0; j < points.length; j++) {
          var p2 = points[j];
          if (p1 === p2) continue;
          var placed = false;
          for (var k = 0; k < 5; k++) {
            if (!placed) {
              if (closest[k] === undefined) {
                closest[k] = p2;
                placed = true;
              }
            }
          }
          for (var k = 0; k < 5; k++) {
            if (!placed) {
              if (getDistance(p1, p2) < getDistance(p1, closest[k])) {
                closest[k] = p2;
                placed = true;
              }
            }
          }
        }
        p1.closest = closest;
      }

      // assign a circle to each point
      for (var i in points) {
        points[i].circle = new Circle(points[i], 2 + Math.random() * 2, 'rgba(0,0,0,0.2)');
      }
    }

    // Event handling
    function addListeners() {
      if (!('ontouchstart' in window)) {
        window.addEventListener('mousemove', mouseMove);
      }
      window.addEventListener('resize', resize);
    }

    function mouseMove(e) {
      var posx = 0, posy = 0;
      if (e.pageX || e.pageY) {
        posx = e.pageX - (document.body.scrollLeft + document.documentElement.scrollLeft);
        posy = e.pageY - (document.body.scrollTop + document.documentElement.scrollTop);
      } else if (e.clientX || e.clientY) {
        posx = e.clientX;
        posy = e.clientY;
      }
      target.x = posx;
      target.y = posy;
    }

    function scrollCheck() {
      // keep animation active regardless of scroll so background stays visible while scrolling
      animateHeader = true;
    }

    function resize() {
      // rebuild points on resize for better distribution
      initHeader();
    }

    // animation
    function initAnimation() {
      for (var i in points) {
        shiftPoint(points[i]);
      }
      animate();
    }

    function animate() {
      if (animateHeader) {
        ctx.clearRect(0, 0, width, height);

        for (var i in points) {
          var p = points[i];

          // update shifting animation state
          if (p.startTime) {
            var elapsed = Date.now() - p.startTime;
            var t = Math.min(1, elapsed / p.duration);
            var tEased = easeInOutCubic(t);
            p.x = p.startX + (p.destX - p.startX) * tEased;
            p.y = p.startY + (p.destY - p.startY) * tEased;
            if (t >= 1) {
              // start next shift
              shiftPoint(p);
            }
          }

          var dist = Math.abs(getDistance(target, p));
          if (dist < 4000) {
            p.active = 0.3;
            p.circle.active = 0.6;
          } else if (dist < 20000) {
            p.active = 0.1;
            p.circle.active = 0.3;
          } else if (dist < 40000) {
            p.active = 0.02;
            p.circle.active = 0.1;
          } else {
            // keep a small baseline visibility so movement is visible even when mouse is static/far
            p.active = 0.015;
            p.circle.active = 0.04;
          }

          // enforce a very small minimum so the subtle shifting is always rendered
          p.active = Math.max(p.active, 0.015);
          p.circle.active = Math.max(p.circle.active, 0.015);

          drawLines(p);
          p.circle.draw();
        }
      }
      requestAnimationFrame(animate);
    }

    function shiftPoint(p) {
      p.startTime = Date.now();
      p.startX = p.x;
      p.startY = p.y;
      p.destX = p.originX - 50 + Math.random() * 100;
      p.destY = p.originY - 50 + Math.random() * 100;
      p.duration = 1000 + Math.random() * 1000; // 1s to 2s
    }

    // Canvas manipulation
    function drawLines(p) {
      if (!p.active) return;
      for (var i in p.closest) {
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        ctx.lineTo(p.closest[i].x, p.closest[i].y);
        ctx.strokeStyle = 'rgba(164,164,164,' + p.active + ')';
        ctx.stroke();
      }
    }

    function Circle(pos, rad, color) {
      var _this = this;
      (function () {
        _this.pos = pos || null;
        _this.radius = rad || null;
        _this.color = color || null;
        _this.active = 0;
      })();

      this.draw = function () {
        if (!_this.active) return;
        ctx.beginPath();
        ctx.arc(_this.pos.x, _this.pos.y, _this.radius, 0, 2 * Math.PI, false);
        ctx.fillStyle = 'rgba(164,164,164,' + _this.active + ')';
        ctx.fill();
      };
    }

    // Util
    function getDistance(p1, p2) {
      return Math.pow(p1.x - p2.x, 2) + Math.pow(p1.y - p2.y, 2);
    }

    function easeInOutCubic(t) {
      return t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;
    }
  }

  // Auto-init when DOM ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () {
      try { backgroundAnimation(); } catch (e) { console.error(e); }
    });
  } else {
    try { backgroundAnimation(); } catch (e) { console.error(e); }
  }
})();
