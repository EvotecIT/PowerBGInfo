// PowerBGInfo site behaviour (forked from the ChartForgeX site): theme switch, code blocks, tables, the docs outline, the surface stage, tabs,
// and the gallery filter and viewer.
(function () {
  'use strict';

  var root = document.documentElement;
  var theme = function () { return root.dataset.theme === 'dark' ? 'dark' : 'light'; };
  var themeListeners = [];

  document.querySelectorAll('[data-pb-theme-toggle]').forEach(function (button) {
    button.addEventListener('click', function () {
      var next = theme() === 'dark' ? 'light' : 'dark';
      root.dataset.theme = next;
      try { localStorage.setItem('pb-theme', next); } catch (e) { /* storage unavailable */ }
      themeListeners.forEach(function (listener) { listener(next); });
    });
  });

  // Header navigation. Each group button discloses its panel; one panel is open at a time on wide screens, and
  // Escape, a click outside or tabbing out of the group closes it. The phone sheet lists every group open.
  var header = document.querySelector('[data-pb-header]');
  var compactNav = window.matchMedia('(max-width: 960px)');
  var navToggle = document.getElementById('pb-nav-toggle');

  function wireNav() {
    var groups = Array.prototype.slice.call(header.querySelectorAll('[data-pb-navgroup]'));
    var triggerOf = function (group) { return group.querySelector('.pb-navgroup__trigger'); };
    var setOpen = function (group, open) { triggerOf(group).setAttribute('aria-expanded', open ? 'true' : 'false'); };
    var closeAll = function (except) { groups.forEach(function (group) { if (group !== except) setOpen(group, false); }); };

    groups.forEach(function (group) {
      var trigger = triggerOf(group);
      var items = Array.prototype.slice.call(group.querySelectorAll('.pb-navitem__link'));
      trigger.addEventListener('click', function () {
        var open = trigger.getAttribute('aria-expanded') !== 'true';
        if (!compactNav.matches) closeAll(group);
        setOpen(group, open);
      });
      trigger.addEventListener('keydown', function (event) {
        if (event.key !== 'ArrowDown' || compactNav.matches || !items.length) return;
        event.preventDefault();
        event.stopPropagation();
        closeAll(group);
        setOpen(group, true);
        items[0].focus();
      });
      group.addEventListener('keydown', function (event) {
        var index = items.indexOf(document.activeElement);
        if (event.key === 'Escape' && !compactNav.matches && trigger.getAttribute('aria-expanded') === 'true') {
          setOpen(group, false);
          trigger.focus();
        } else if (index >= 0 && !compactNav.matches && /^Arrow(Left|Right|Up|Down)$/.test(event.key)) {
          event.preventDefault();
          var step = event.key === 'ArrowLeft' || event.key === 'ArrowUp' ? -1 : 1;
          items[(index + step + items.length) % items.length].focus();
        }
      });
      group.addEventListener('focusout', function (event) {
        if (!compactNav.matches && event.relatedTarget && !group.contains(event.relatedTarget)) setOpen(group, false);
      });
    });

    document.addEventListener('click', function (event) {
      if (!compactNav.matches && !header.contains(event.target)) closeAll(null);
    });
    var reset = function () { if (compactNav.matches) groups.forEach(function (group) { setOpen(group, true); }); else closeAll(null); };
    if (compactNav.addEventListener) compactNav.addEventListener('change', reset);
    reset();
  }

  if (navToggle) {
    document.addEventListener('keydown', function (event) {
      if (event.key === 'Escape' && navToggle.checked && compactNav.matches) { navToggle.checked = false; navToggle.focus(); }
    });
  }

  if (header && header.querySelector('[data-pb-navgroup]')) wireNav();

  function copyText(text, button) {
    var label = button.getAttribute('data-label') || button.textContent;
    button.setAttribute('data-label', label);
    var done = function () {
      button.setAttribute('data-copied', '');
      setLabel(button, 'Copied');
      setTimeout(function () { button.removeAttribute('data-copied'); setLabel(button, label); }, 1600);
    };
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text).then(done, function () {});
    }
  }

  function setLabel(button, text) {
    var span = button.querySelector('[data-pb-label]');
    if (span) span.textContent = text; else button.textContent = text;
  }

  document.querySelectorAll('[data-pb-copy]').forEach(function (button) {
    button.addEventListener('click', function () { copyText(button.getAttribute('data-pb-copy'), button); });
  });

  // Syntax highlighting. PowerForge injects Prism on pages whose Markdown has code; pages that render code from
  // templates or at runtime load the same self-hosted copy on demand.
  var PRISM_ROOT = '/assets/prism/';
  var prismReady = null;

  function loadScript(src) {
    return new Promise(function (resolve, reject) {
      var script = document.createElement('script');
      script.src = src;
      script.onload = resolve;
      script.onerror = reject;
      document.head.appendChild(script);
    });
  }

  function ensurePrism() {
    var configure = function () {
      var prism = window.Prism;
      // Native authoring fences use Prism's existing plain-text grammar, including inside Markdown examples.
      prism.languages.chartforgex = prism.languages.plain;
      if (prism.plugins && prism.plugins.autoloader) prism.plugins.autoloader.languages_path = PRISM_ROOT + 'components/';
      return prism;
    };
    // Share an in-flight load even when core has finished but the autoloader has not.
    if (prismReady) return prismReady;
    var prism = window.Prism;
    if (prism && prism.highlightElement && prism.plugins && prism.plugins.autoloader) {
      return Promise.resolve(configure());
    }
    window.Prism = window.Prism || {};
    window.Prism.manual = true;
    var coreReady = prism && prism.highlightElement
      ? Promise.resolve() : loadScript(PRISM_ROOT + 'prism-core.js');
    prismReady = coreReady.then(function () {
      var loaded = window.Prism;
      if (!loaded || !loaded.util || !loaded.highlightElement) throw new Error('Prism core did not initialize.');
      if (!loaded.plugins || !loaded.plugins.autoloader) return loadScript(PRISM_ROOT + 'prism-autoloader.js');
    }).then(configure).catch(function (error) {
      // A later source-tab interaction can retry a failed component without reloading healthy core.
      prismReady = null;
      throw error;
    });
    return prismReady;
  }

  function highlight(code) {
    if (!code || !/\blanguage-/.test(code.className)) return;
    ensurePrism().then(function (prism) { prism.highlightElement(code); }, function () { /* plain text stays readable */ });
  }
  window.pbHighlight = highlight;

  // One code block component: language key, optional file name and a copy action in a caption bar.
  var LANGUAGES = {
    csharp: 'C#', cs: 'C#', powershell: 'PowerShell', ps1: 'PowerShell', bash: 'Shell', shell: 'Shell', sh: 'Shell',
    json: 'JSON', xml: 'XML', markup: 'HTML', html: 'HTML', markdown: 'Markdown', md: 'Markdown', mermaid: 'Mermaid',
    yaml: 'YAML', css: 'CSS', javascript: 'JavaScript', js: 'JavaScript', typescript: 'TypeScript', chartforgex: 'ChartForgeX markup', text: 'Text'
  };
  var COPY_ICON = '<svg viewBox="0 0 16 16" width="13" height="13" aria-hidden="true"><rect x="5.5" y="5.5" width="8" height="8" rx="1.5" fill="none" stroke="currentColor" stroke-width="1.4"/><path d="M10.5 3.5v-.5A1.5 1.5 0 0 0 9 1.5H4A1.5 1.5 0 0 0 2.5 3v5A1.5 1.5 0 0 0 4 9.5h.5" fill="none" stroke="currentColor" stroke-width="1.4"/></svg>';

  function languageOf(code) {
    var match = /\blanguage-([\w-]+)/.exec(code ? code.className : '');
    return match ? match[1].toLowerCase() : 'text';
  }

  function copyButton(getText) {
    var button = document.createElement('button');
    button.type = 'button';
    button.className = 'pb-codeblock__btn';
    button.setAttribute('data-pb-code-copy', '');
    button.innerHTML = COPY_ICON + '<span data-pb-label>Copy</span>';
    button.addEventListener('click', function () { copyText(getText(), button); });
    return button;
  }

  function wireCodeBlock(block) {
    var pre = block.querySelector('pre');
    var code = pre && pre.querySelector('code');
    if (!code) return;
    var actions = block.querySelector('.pb-codeblock__actions');
    if (actions && !actions.querySelector('[data-pb-code-copy]')) {
      actions.appendChild(copyButton(function () { return block.pbSource || code.textContent; }));
    }
    highlight(code);
  }

  function enhancePre(pre) {
    if (pre.closest('.pb-codeblock') || pre.closest('[data-pb-plain-code]')) return;
    var code = pre.querySelector('code');
    if (!code) return;
    var language = languageOf(code);
    var block = document.createElement('figure');
    block.className = 'pb-codeblock';
    block.setAttribute('data-lang', language);
    var bar = document.createElement('figcaption');
    bar.className = 'pb-codeblock__bar';
    bar.innerHTML = '<span class="pb-codeblock__lang"></span><span class="pb-codeblock__actions"></span>';
    bar.firstChild.textContent = LANGUAGES[language] || language;
    pre.parentNode.insertBefore(block, pre);
    block.appendChild(bar);
    block.appendChild(pre);
    wireCodeBlock(block);
  }

  // Help text indents example code to its own margin and keeps only the first line flush, so the common indent of
  // the remaining lines is removed (or of every line when the first is indented too).
  function dedent(text) {
    var lines = text.replace(/\s+$/, '').replace(/^\s*\n/, '').split('\n');
    var indent = function (line) { return /^[ \t]*/.exec(line)[0].length; };
    var from = lines.length > 1 && indent(lines[0]) === 0 ? 1 : 0;
    var min = Infinity;
    lines.slice(from).forEach(function (line) { if (line.trim()) min = Math.min(min, indent(line)); });
    if (!isFinite(min) || min === 0) return lines.join('\n');
    return lines.map(function (line, index) { return index < from ? line : line.slice(Math.min(min, indent(line))); }).join('\n');
  }

  document.querySelectorAll('.pb-codeblock').forEach(wireCodeBlock);
  document.querySelectorAll('.pb-prose pre').forEach(enhancePre);

  // API and command reference examples join the same code block. The bare "PS>" line before each one is dropped;
  // the block's language label says what it is.
  document.querySelectorAll('.pf-api-docs .type-examples pre').forEach(function (pre) {
    var code = pre.querySelector('code');
    if (!code) return;
    code.textContent = dedent(code.textContent);
    var prompt = pre.previousElementSibling;
    if (prompt && prompt.tagName === 'P' && /^PS>$/.test(prompt.textContent.trim())) prompt.remove();
    enhancePre(pre);
  });

  // Wide tables scroll inside their own frame instead of widening the page.
  document.querySelectorAll('.pb-prose table').forEach(function (table) {
    if (table.parentElement.classList.contains('pb-tablewrap')) return;
    var wrap = document.createElement('div');
    wrap.className = 'pb-tablewrap';
    wrap.tabIndex = 0;
    wrap.setAttribute('role', 'region');
    wrap.setAttribute('aria-label', 'Table');
    table.parentNode.insertBefore(wrap, table);
    wrap.appendChild(table);
  });

  // The docs contents start folded on narrow screens so the article comes first.
  if (window.matchMedia && window.matchMedia('(max-width: 820px)').matches) {
    document.querySelectorAll('.pb-docs__menu[open]').forEach(function (menu) { menu.open = false; });
  }

  // Docs outline: the heading in view is marked on the outline's axis.
  var outline = document.querySelector('[data-pb-outline]');
  if (outline && 'IntersectionObserver' in window) {
    var links = Array.prototype.slice.call(outline.querySelectorAll('a[href^="#"]'));
    var targets = links.map(function (link) { return document.getElementById(decodeURIComponent(link.getAttribute('href').slice(1))); });
    var visible = new Map();
    var mark = function () {
      var active = null;
      targets.forEach(function (target, index) { if (!active && target && visible.get(target)) active = links[index]; });
      if (!active) return;
      links.forEach(function (link) { if (link === active) link.setAttribute('aria-current', 'location'); else link.removeAttribute('aria-current'); });
    };
    var observer = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) { visible.set(entry.target, entry.isIntersecting); });
      mark();
    }, { rootMargin: '-80px 0px -60% 0px' });
    targets.forEach(function (target) { if (target) observer.observe(target); });
  }

  // Accessible tabs: arrow keys move between tabs, Home/End jump to the ends.
  function wireTabs(tabs, onSelect) {
    tabs.forEach(function (tab, index) {
      tab.addEventListener('click', function () { onSelect(tab); });
      tab.addEventListener('keydown', function (event) {
        var next = null;
        if (event.key === 'ArrowRight') next = tabs[(index + 1) % tabs.length];
        if (event.key === 'ArrowLeft') next = tabs[(index - 1 + tabs.length) % tabs.length];
        if (event.key === 'Home') next = tabs[0];
        if (event.key === 'End') next = tabs[tabs.length - 1];
        if (next) { event.preventDefault(); next.focus(); onSelect(next); }
      });
    });
  }

  function selectTab(tabs, active) {
    tabs.forEach(function (tab) {
      var selected = tab === active;
      tab.setAttribute('aria-selected', selected ? 'true' : 'false');
      tab.tabIndex = selected ? 0 : -1;
      var panel = document.getElementById(tab.getAttribute('aria-controls'));
      if (panel) {
        panel.hidden = !selected;
        if (selected) panel.querySelectorAll('code').forEach(highlight);
      }
    });
  }

  // The hero stage: one chart, every surface.
  var stage = document.querySelector('[data-pb-stage]');
  if (stage) {
    var stageTabs = Array.prototype.slice.call(stage.querySelectorAll('[role="tab"]'));
    var frame = stage.querySelector('[data-pb-html-frame]');
    var url = stage.querySelector('[data-pb-html-url]');
    var htmlMode = 'static';

    var loadFrame = function () {
      if (!frame) return;
      var file = 'hero-' + theme() + (htmlMode === 'interactive' ? '.interactive' : '') + '.html';
      if (frame.getAttribute('data-file') !== file) {
        frame.setAttribute('data-file', file);
        frame.src = '/site/' + file;
      }
      if (url) url.textContent = htmlMode === 'interactive' ? 'sign-ins.interactive.html' : 'sign-ins.html';
    };

    wireTabs(stageTabs, function (tab) {
      selectTab(stageTabs, tab);
      var id = tab.getAttribute('data-pb-surface');
      stage.querySelectorAll('[data-pb-caption]').forEach(function (caption) {
        caption.hidden = caption.getAttribute('data-pb-caption') !== id;
      });
      if (id === 'html') loadFrame();
    });

    stage.querySelectorAll('[data-pb-html-mode]').forEach(function (button) {
      button.addEventListener('click', function () {
        htmlMode = button.getAttribute('data-pb-html-mode');
        stage.querySelectorAll('[data-pb-html-mode]').forEach(function (other) {
          other.setAttribute('aria-pressed', other === button ? 'true' : 'false');
        });
        loadFrame();
      });
    });

    themeListeners.push(function () {
      var htmlPanel = document.getElementById('pb-panel-html');
      if (htmlPanel && !htmlPanel.hidden) loadFrame();
      else if (frame) frame.removeAttribute('data-file');
    });
  }

  // Generic tab groups (Mermaid samples).
  document.querySelectorAll('[data-pb-tabs]').forEach(function (group) {
    var tabs = Array.prototype.slice.call(group.querySelectorAll('[role="tab"]'));
    wireTabs(tabs, function (tab) { selectTab(tabs, tab); });
  });

  // Gallery filter: the category legend.
  document.querySelectorAll('[data-pb-filters]').forEach(function (filters) {
    var grid = document.querySelector('[data-pb-gallery]');
    filters.querySelectorAll('button').forEach(function (button) {
      button.addEventListener('click', function () {
        var value = button.getAttribute('data-filter');
        filters.querySelectorAll('button').forEach(function (other) {
          other.setAttribute('aria-pressed', other === button ? 'true' : 'false');
        });
        grid.querySelectorAll('.pb-card').forEach(function (card) {
          card.hidden = value !== 'all' && card.getAttribute('data-category') !== value;
        });
      });
    });
  });

  // Viewer dialog for gallery cards (gallery-card.html). A card names its output in each theme, the full-size file,
  // a page link and its source file; gallery sources are standalone .csharp.txt programs. A card may also carry tabs,
  // line ranges of that one source (PowerBGInfo shows a script's presentation and data blocks). A card without
  // data-image shows its own image for the page theme at the viewer's size, so the viewer uses whatever responsive
  // variants the build wrote for it.

  var viewer = document.querySelector('[data-pb-viewer]');
  if (viewer && typeof viewer.showModal === 'function') {
    var current = null;
    var opener = null;
    var request = 0;
    var tabIndex = 0;
    var sources = {};
    var image = viewer.querySelector('[data-pb-viewer-image]');
    var code = viewer.querySelector('[data-pb-viewer-code]');
    var block = code.closest('.pb-codeblock');
    var tablist = viewer.querySelector('[data-pb-viewer-tabs]');
    var openLink = viewer.querySelector('[data-pb-viewer-open]');
    var pageLink = viewer.querySelector('[data-pb-viewer-page]');
    var download = viewer.querySelector('[data-pb-viewer-download]');
    var pick = function (light, darkValue) { return theme() === 'dark' && darkValue ? darkValue : light; };
    var fileName = function (url) { return url.split('/').pop().replace(/\.csharp\.txt$/, '.cs').replace(/\.txt$/, ''); };

    var loadSource = function (url) {
      if (!sources[url]) {
        sources[url] = fetch(url).then(function (response) { return response.ok ? response.text() : Promise.reject(); });
        sources[url].catch(function () { delete sources[url]; });
      }
      return sources[url];
    };

    var showSource = function () {
      var source = pick(current.source, current.darkSource);
      var range = current.tabs ? current.tabs[tabIndex] : null;
      var ticket = ++request;
      download.href = source;
      download.setAttribute('download', fileName(source));
      viewer.querySelector('[data-pb-viewer-file]').textContent = fileName(source);
      block.pbSource = '';
      code.textContent = 'Loading source…';
      loadSource(source)
        .then(function (text) {
          if (ticket !== request) return;
          if (range && range.from) text = text.split('\n').slice(range.from - 1, range.to).join('\n') + '\n';
          block.pbSource = text;
          code.textContent = text;
          highlight(code);
        })
        .catch(function () {
          if (ticket !== request) return;
          code.textContent = 'The source file could not be loaded. Use the links above instead.';
        });
    };

    var buildTabs = function () {
      tablist.textContent = '';
      tablist.hidden = !current.tabs;
      tabIndex = 0;
      if (!current.tabs) {
        block.removeAttribute('role');
        block.removeAttribute('aria-labelledby');
        return;
      }
      block.setAttribute('role', 'tabpanel');
      var tabs = current.tabs.map(function (entry, index) {
        var tab = document.createElement('button');
        tab.type = 'button';
        tab.id = 'pb-viewer-tab-' + index;
        tab.setAttribute('role', 'tab');
        tab.setAttribute('aria-controls', block.id);
        tab.innerHTML = '<span class="pb-key' + (index ? ' pb-key--hollow' : '') + '" aria-hidden="true"></span>';
        tab.appendChild(document.createTextNode(entry.label));
        tablist.appendChild(tab);
        return tab;
      });
      var choose = function (active, load) {
        tabIndex = tabs.indexOf(active);
        tabs.forEach(function (tab) {
          tab.setAttribute('aria-selected', tab === active ? 'true' : 'false');
          tab.tabIndex = tab === active ? 0 : -1;
        });
        block.setAttribute('aria-labelledby', active.id);
        if (load !== false) showSource();
      };
      wireTabs(tabs, choose);
      choose(tabs[0], false);
    };

    var render = function () {
      if (!current) return;
      var own = current.image ? null : opener.querySelector(theme() === 'dark' ? 'img.pb-only-dark' : 'img.pb-only-light');
      if (own) {
        image.sizes = viewer.getAttribute('data-sizes') || '100vw';
        image.srcset = own.getAttribute('srcset') || '';
        image.src = own.getAttribute('src');
        image.width = +own.getAttribute('width');
        image.height = +own.getAttribute('height');
      } else {
        ['srcset', 'sizes', 'width', 'height'].forEach(function (name) { image.removeAttribute(name); });
        image.src = pick(current.image, current.darkImage);
      }
      image.alt = current.alt || current.title + ', ' + current.family + ' rendered by PowerBGInfo';
      viewer.querySelector('[data-pb-viewer-title]').textContent = current.title;
      viewer.querySelector('[data-pb-viewer-family]').textContent = [current.family, current.note].filter(Boolean).join(' · ');
      openLink.href = pick(current.full, current.darkFull) || image.getAttribute('src');
      var page = pick(current.page, current.darkPage);
      pageLink.hidden = !page;
      if (page) pageLink.href = page;
      showSource();
    };

    document.querySelectorAll('[data-pb-gallery-open]').forEach(function (button) {
      button.addEventListener('click', function () {
        var d = button.dataset;
        var tabs = null;
        try { tabs = d.tabs ? JSON.parse(d.tabs) : null; } catch (e) { tabs = null; }
        current = {
          title: d.title, family: d.family, note: d.note, alt: d.alt, image: d.image, darkImage: d.darkImage, full: d.full,
          darkFull: d.darkFull, source: d.source, darkSource: d.darkSource, page: d.page, darkPage: d.darkPage, tabs: tabs
        };
        opener = button;
        var card = button.closest('.pb-card');
        var key = card ? card.style.getPropertyValue('--key') : '';
        if (key) viewer.style.setProperty('--key', key); else viewer.style.removeProperty('--key');
        buildTabs();
        render();
        lockPage();
        viewer.showModal();
      });
    });

    // While the viewer is open the page behind it must not scroll: the wheel or a touch drag that reaches the end of
    // the dialog's own scrolling would otherwise move the page. Lock the root, keep the scrollbar's space so nothing
    // shifts, and put everything back on close (Escape, the close button and a backdrop click all end in "close").
    var lockedScroll = null;
    function lockPage() {
      if (lockedScroll) { return; }
      var root = document.documentElement;
      lockedScroll = { x: window.scrollX, y: window.scrollY, overflow: root.style.overflow, gutter: root.style.scrollbarGutter };
      root.style.scrollbarGutter = 'stable';
      root.style.overflow = 'hidden';
      root.classList.add('pb-viewer-open');
    }
    function unlockPage() {
      if (!lockedScroll) { return; }
      var root = document.documentElement;
      root.style.overflow = lockedScroll.overflow;
      root.style.scrollbarGutter = lockedScroll.gutter;
      root.classList.remove('pb-viewer-open');
      window.scrollTo(lockedScroll.x, lockedScroll.y);
      lockedScroll = null;
    }
    // Every close path unlocks the page at once. The "close" event can arrive late, even after the viewer has been
    // opened again, so it only backs up the direct paths and ignores itself while the viewer is open.
    function finishClose() {
      unlockPage();
      if (opener && opener.isConnected) opener.focus({ preventScroll: true });
    }
    function closeViewer() {
      if (viewer.open) viewer.close();
      finishClose();
    }
    viewer.querySelector('[data-pb-viewer-close]').addEventListener('click', closeViewer);
    viewer.addEventListener('click', function (event) { if (event.target === viewer) closeViewer(); });
    // Use the same synchronous close path for Escape: unlocking before the native close can scroll the background.
    viewer.addEventListener('cancel', function (event) { event.preventDefault(); closeViewer(); });
    viewer.addEventListener('close', function () { if (!viewer.open) finishClose(); });
    themeListeners.push(function () { if (viewer.open) render(); });
  }
})();

// Home monitor: the scenario chips under the monitor switch which wallpaper it shows. Each scenario is its own image
// with its own responsive sources, so the browser picks the right file for the new one; the chips only change which
// image is visible. Each chip is a link to that scenario in the gallery, so without JavaScript it still goes somewhere.
(function () {
  document.querySelectorAll('[data-pbg-monitor]').forEach(function (monitor) {
    var images = Array.prototype.slice.call(monitor.querySelectorAll('[data-pbg-screen]'));
    var file = monitor.querySelector('[data-pbg-screen-file]');
    var chips = Array.prototype.slice.call(monitor.querySelectorAll('[data-pbg-scene]'));
    if (!images.length || !chips.length) { return; }
    chips.forEach(function (chip) {
      chip.addEventListener('click', function (event) {
        if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) { return; }
        var scene = chip.getAttribute('data-pbg-scene');
        var target = null;
        images.forEach(function (image) { if (image.getAttribute('data-pbg-screen') === scene) { target = image; } });
        if (!target) { return; }
        event.preventDefault();
        images.forEach(function (image) { image.hidden = image !== target; });
        chips.forEach(function (other) {
          if (other === chip) { other.setAttribute('aria-current', 'true'); }
          else { other.removeAttribute('aria-current'); }
        });
        if (file) { file.textContent = chip.getAttribute('data-file'); }
      });
    });
  });
})();

// Image viewer: a click on a wallpaper render opens it large in a dialog instead of navigating to the PNG. Previous
// and next step through the related renders in the same block (the build steps, the theme presets, the regions or
// the featured scenarios); the arrow keys and a swipe do the same. The link itself stays, so a modified click, a
// middle click, "save image" and pages without script still reach the file.
(function () {
  if (typeof HTMLDialogElement !== 'function') { return; }
  var links = document.querySelectorAll('a.pb-figure__open');
  if (!links.length) { return; }

  var dialog = document.createElement('dialog');
  dialog.className = 'pb-lightbox';
  dialog.setAttribute('aria-label', 'Image viewer');
  dialog.innerHTML =
    '<div class="pb-lightbox__bar">' +
      '<div class="pb-lightbox__titles"><p class="pb-lightbox__title" data-lb-title></p><p class="pb-lightbox__count" data-lb-count></p></div>' +
      '<div class="pb-lightbox__links"><a class="pb-lightbox__link" data-lb-full target="_blank" rel="noopener">Full size</a>' +
      '<a class="pb-lightbox__link" data-lb-download download>Download</a></div>' +
      '<button type="button" class="pb-lightbox__close" data-lb-close aria-label="Close viewer">' +
        '<svg viewBox="0 0 20 20" width="18" height="18" aria-hidden="true"><path d="M5 5l10 10M15 5L5 15" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/></svg></button>' +
    '</div>' +
    '<div class="pb-lightbox__stage">' +
      '<button type="button" class="pb-lightbox__nav pb-lightbox__nav--prev" data-lb-prev aria-label="Previous image">' +
        '<svg viewBox="0 0 20 20" width="20" height="20" aria-hidden="true"><path d="M12.5 4.5 7 10l5.5 5.5" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg></button>' +
      '<figure class="pb-lightbox__figure"><img data-lb-image alt="" decoding="async" /><figcaption class="pb-lightbox__caption" data-lb-caption></figcaption></figure>' +
      '<button type="button" class="pb-lightbox__nav pb-lightbox__nav--next" data-lb-next aria-label="Next image">' +
        '<svg viewBox="0 0 20 20" width="20" height="20" aria-hidden="true"><path d="M7.5 4.5 13 10l-5.5 5.5" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg></button>' +
    '</div>';
  document.body.appendChild(dialog);

  var image = dialog.querySelector('[data-lb-image]');
  var title = dialog.querySelector('[data-lb-title]');
  var count = dialog.querySelector('[data-lb-count]');
  var caption = dialog.querySelector('[data-lb-caption]');
  var full = dialog.querySelector('[data-lb-full]');
  var download = dialog.querySelector('[data-lb-download]');
  var prev = dialog.querySelector('[data-lb-prev]');
  var next = dialog.querySelector('[data-lb-next]');
  var stage = dialog.querySelector('.pb-lightbox__stage');
  var group = [];
  var index = 0;
  var opener = null;
  var locked = null;
  // Until the visitor scrolls or steps, the stage stays at its top while the image loads and grows above the caption.
  var settling = false;

  function visible(link) { return link.getClientRects().length > 0 && getComputedStyle(link).display !== 'none'; }
  // A render belongs to its nearest block; renders inside a nested block (a .pbg-block inside a section) stay out of
  // the outer group, so the featured scenarios do not mix with the block of screens under them.
  function boxOf(link) { return link.closest('[data-pb-lightbox-group], .pbg-block, section.pb-atlas') || document.body; }
  function groupOf(link) {
    var box = boxOf(link);
    return Array.prototype.filter.call(box.querySelectorAll('a.pb-figure__open'), function (l) { return boxOf(l) === box && visible(l); });
  }
  function text(node) { return node ? node.textContent.replace(/\s+/g, ' ').trim() : ''; }
  // The title is the heading the render sits under (a build step, a scenario, a widget); the caption parts (theme
  // name, file, size) follow the count, or become the title when there is no heading.
  function describe(link) {
    var img = link.querySelector('img');
    var item = link.closest('li, section.pb-atlas__group, .pbg-block');
    var head = text(item && item.querySelector('h3, h4'));
    var figure = link.closest('figure');
    var cap = figure && figure.querySelector(':scope > figcaption');
    var parts = cap ? Array.prototype.map.call(cap.children, text).filter(Boolean) : [];
    if (cap && !parts.length && text(cap)) { parts = [text(cap)]; }
    return { head: head, caption: parts.join(' \u00b7 '), alt: img ? img.getAttribute('alt') || '' : '', img: img };
  }
  function show(i) {
    index = (i + group.length) % group.length;
    var link = group[index];
    var info = describe(link);
    var href = link.getAttribute('href');
    image.removeAttribute('srcset');
    if (info.img && info.img.getAttribute('srcset')) {
      image.setAttribute('srcset', info.img.getAttribute('srcset'));
      image.setAttribute('sizes', '(max-width: 900px) 100vw, 90vw');
    }
    image.src = href;
    image.alt = info.alt;
    var file = href.split('/').pop();
    title.textContent = info.head || info.caption || file;
    caption.textContent = info.alt;
    caption.hidden = !info.alt;
    count.textContent = [group.length > 1 ? (index + 1) + ' of ' + group.length : '', info.head ? info.caption || file : file]
      .filter(Boolean).filter(function (v, n, all) { return all.indexOf(v) === n && v !== title.textContent; }).join(' \u00b7 ');
    full.href = href;
    download.href = href;
    download.setAttribute('download', href.split('/').pop());
    prev.hidden = next.hidden = group.length < 2;
    // Warm the neighbours so stepping feels instant.
    [index - 1, index + 1].forEach(function (n) {
      if (group.length < 2) { return; }
      var l = group[(n + group.length) % group.length];
      var pre = new Image(); pre.src = l.getAttribute('href');
    });
  }
  function lock() {
    if (locked) { return; }
    var root = document.documentElement;
    locked = { x: window.scrollX, y: window.scrollY, overflow: root.style.overflow, gutter: root.style.scrollbarGutter };
    root.style.scrollbarGutter = 'stable';
    root.style.overflow = 'hidden';
  }
  function unlock() {
    if (!locked) { return; }
    var root = document.documentElement;
    root.style.overflow = locked.overflow;
    root.style.scrollbarGutter = locked.gutter;
    window.scrollTo(locked.x, locked.y);
    locked = null;
  }
  function finish() {
    unlock();
    if (opener && opener.isConnected) { opener.focus({ preventScroll: true }); }
  }
  function close() {
    if (dialog.open) { dialog.close(); }
    finish();
  }

  Array.prototype.forEach.call(links, function (link) {
    link.addEventListener('click', function (event) {
      if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) { return; }
      event.preventDefault();
      group = groupOf(link);
      if (!group.length) { group = [link]; }
      opener = link;
      show(Math.max(0, group.indexOf(link)));
      lock();
      dialog.showModal();
      // Focus stays in the bar. On a short screen the arrows sit in the scrolling stage under the caption, and WebKit
      // scrolls a focused control there into view even with preventScroll, which hides the image. The arrow keys step
      // through the dialog's own key handler, so nothing needs focus inside the stage.
      dialog.querySelector('[data-lb-close]').focus({ preventScroll: true });
      settling = true;
      stage.scrollTop = 0;
    });
  });

  image.addEventListener('load', function () { if (settling) { stage.scrollTop = 0; } });
  ['wheel', 'touchstart', 'pointerdown', 'keydown'].forEach(function (type) {
    dialog.addEventListener(type, function () { settling = false; }, { passive: true });
  });
  prev.addEventListener('click', function () { show(index - 1); });
  next.addEventListener('click', function () { show(index + 1); });
  dialog.querySelector('[data-lb-close]').addEventListener('click', close);
  dialog.addEventListener('click', function (event) {
    if (event.target === dialog || event.target.classList.contains('pb-lightbox__stage')) { close(); }
  });
  dialog.addEventListener('keydown', function (event) {
    if (event.key === 'ArrowRight') { event.preventDefault(); show(index + 1); }
    else if (event.key === 'ArrowLeft') { event.preventDefault(); show(index - 1); }
    else if (event.key === 'Home') { event.preventDefault(); show(0); }
    else if (event.key === 'End') { event.preventDefault(); show(group.length - 1); }
  });
  // Escape: "cancel" fires before the dialog closes itself; "close" can arrive late, so it only backs up the rest.
  dialog.addEventListener('cancel', unlock);
  dialog.addEventListener('close', function () { if (!dialog.open) { finish(); } });

  // Swipe left or right on the image to step.
  var startX = null;
  var startY = null;
  image.addEventListener('pointerdown', function (event) { startX = event.clientX; startY = event.clientY; });
  image.addEventListener('pointerup', function (event) {
    if (startX === null || group.length < 2) { return; }
    var dx = event.clientX - startX;
    var dy = event.clientY - startY;
    startX = startY = null;
    if (Math.abs(dx) > 40 && Math.abs(dx) > Math.abs(dy)) { show(index + (dx < 0 ? 1 : -1)); }
  });
})();
