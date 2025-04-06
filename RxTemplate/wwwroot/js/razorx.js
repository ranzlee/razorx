var razorx = (function () {
  "use strict";

  var _debug = false;

  var printDebug = function (func, msg) {
    if (_debug !== true) {
      return;
    }
    console.info(`razorx: [${func}] ${msg}`);
  };

  var printError = function (func, msg) {
    console.error(`razorx: [${func}] ${msg}`);
  };

  var _theme = {
    themeKey: "data-theme",
    lightTheme: "light",
    darkTheme: "dark",

    handleSystemThemeChange: function () {
      window
        .matchMedia("(prefers-color-scheme: dark)")
        .addEventListener("change", _theme.applyTheme);
    },

    configureTheme: function (light, dark) {
      _theme.lightTheme = light;
      _theme.darkTheme = dark;
    },

    getLightTheme: function () {
      return _theme.lightTheme;
    },

    getDarkTheme: function () {
      return _theme.darkTheme;
    },

    applyTheme: function () {
      var root = document.documentElement;
      var t = localStorage.getItem(_theme.themeKey) ?? "";
      if (t === "") {
        if (window.matchMedia("(prefers-color-scheme: dark)").matches) {
          root.setAttribute(_theme.themeKey, _theme.darkTheme);
        } else {
          root.setAttribute(_theme.themeKey, _theme.lightTheme);
        }
      } else {
        root.setAttribute(_theme.themeKey, t);
      }
    },
  };

  var _triggers = {
    handleToastTriggers: function () {
      document.addEventListener("razorx-toast-trigger", function (evt) {
        var obj = evt.detail;
        var live = document.getElementById("aria-live-region");
        if (live) {
          live.innerText = `Last action was: ${obj.Message}`;
        }
        var ele = htmx.find(`${obj.ToastSelector}-template`);
        if (!ele) {
          return;
        }
        ele = ele.content.cloneNode(true);
        ele = htmx.find(ele, obj.ToastSelector);
        if (!ele) {
          return;
        }
        var rnd = Math.floor(Math.random() * 10000);
        ele.id = `${ele.id}-${rnd}`;
        if (!ele.children || ele.children.length === 0) {
          return;
        }
        ele.children.item(0).innerText = obj.Message;
        document.body.prepend(ele);
        var currentToasts = document.querySelectorAll('[data-toast="on"]');
        if (currentToasts.length > 0) {
          currentToasts.forEach((t) => {
            t.setAttribute("data-toast", "off");
            t.style.display = "none";
          });
        }
        ele.setAttribute("data-toast", "on");
        var transitionClass = ele.getAttribute("data-transition-class") ?? "";
        var beginTransitionAfter =
          ele.getAttribute("data-begin-transition-after") ?? "3500";
        var removeAfter = ele.getAttribute("data-remove-after") ?? "4500";
        setTimeout(() => {
          ele.classList.add(transitionClass);
        }, Number(beginTransitionAfter));
        setTimeout(() => {
          ele.remove();
        }, Number(removeAfter));
      });
    },

    handleCloseModalTriggers: function () {
      document.addEventListener("razorx-close-modal-trigger", function (evt) {
        var modal = htmx.find(evt.detail.ModalSelector);
        if (modal) {
          modal.style.display = "none";
          modal.close();
          modal.style.display = "";
        }
      });
    },

    handleFocusTriggers: function () {
      document.addEventListener("razorx-focus-trigger", function (evt) {
        var obj = evt.detail;
        var ele = htmx.find(obj.ElementSelector);
        if (ele) {
          ele.focus();
          if (!obj.ScrollIntoView) {
            return;
          }
          setTimeout(() => {
            ele.scrollIntoView({
              behavior: "instant",
              block: "center",
              inline: "nearest",
            });
          }, 0);
        }
      });
    },
  };

  var _metadata = {
    scope: {
      transient: 0,
      session: 1,
      persistent: 2,
    },

    set: function (scope, key, value) {
      if (scope === _metadata.scope.transient) {
        var input = document.getElementById(key);
        if (input && value) {
          input.value = encodeURIComponent(value);
        }
        return;
      }
      if (scope === _metadata.scope.session) {
        sessionStorage.setItem(key, value);
        return;
      }
      if (scope === _metadata.scope.persistent) {
        localStorage.setItem(key, value);
      }
    },

    handleSetTriggers: function () {
      document.addEventListener("razorx-set-metadata-trigger", function (evt) {
        var obj = evt.detail;
        _metadata.set(obj.Scope, obj.Key, obj.Value);
      });
    },

    handleRemoveTriggers: function () {
      document.addEventListener(
        "razorx-remove-metadata-trigger",
        function (evt) {
          var obj = evt.detail;
          if (obj.Scope === _metadata.scope.transient) {
            var input = document.getElementById(obj.Key);
            if (input) {
              input.value = "";
            }
            return;
          }
          if (obj.Scope === _metadata.scope.session) {
            sessionStorage.removeItem(obj.Key);
            return;
          }
          if (obj.Scope === _metadata.scope.persistent) {
            localStorage.removeItem(obj.Key);
          }
        }
      );
    },

    useHeaderVerbs: ["post", "put", "patch"],

    useStorageMetadata: function (state, key, evt) {
      if (state === "") {
        printDebug("addToRequest", `storage key [${key}] not found or empty.`);
        return;
      }
      if (
        _metadata.useHeaderVerbs.indexOf(
          evt.detail?.verb?.toLowerCase() ?? ""
        ) >= 0
      ) {
        printDebug(
          "addToRequest",
          `adding [${key}] metadata to request headers.`
        );
        evt.detail.headers[key] = state;
        return;
      }
      const params = {
        ...evt.detail.parameters,
        [key]: encodeURIComponent(state),
      };
      printDebug(
        "addToRequest",
        `adding [${key}] metadata to request query parameters.`
      );
      evt.detail.parameters = params;
    },

    useFormMetadata: function (key, evt) {
      var stateElement = document.getElementById(key);
      if (!stateElement) {
        printError("addToRequest", `form element with ID [${key}] not found.`);
        return;
      }
      if (
        _metadata.useHeaderVerbs.indexOf(
          evt.detail?.verb?.toLowerCase() ?? ""
        ) >= 0
      ) {
        printDebug(
          "addToRequest",
          `adding [${key}] metadata to request headers.`
        );
        evt.detail.headers[key] = decodeURIComponent(stateElement.value ?? "");
        return;
      }
      const params = {
        ...evt.detail.parameters,
        [key]: stateElement.value ?? "",
      };
      printDebug(
        "addToRequest",
        `adding [${key}] metadata to request query parameters.`
      );
      evt.detail.parameters = params;
    },

    addToRequest: function (scope, key, triggerElement) {
      if (!triggerElement) {
        printDebug(
          "addToRequest",
          "triggerElement not provided, using document.body."
        );
        triggerElement = document.body;
      }
      function handler(evt) {
        printDebug(
          "addToRequest",
          `event handler triggered for ${key} on ${
            triggerElement === document.body ? "body" : triggerElement?.id
          }.`
        );
        if (scope === _metadata.scope.transient) {
          _metadata.useFormMetadata(key, evt);
        } else {
          var state =
            scope === _metadata.scope.session
              ? sessionStorage.getItem(key) ?? ""
              : localStorage.getItem(key) ?? "";
          _metadata.useStorageMetadata(state, key, evt);
        }
      }
      printDebug(
        "addToRequest",
        `adding event handler for ${key} to ${
          triggerElement === document.body ? "body" : triggerElement?.id
        }.`
      );
      triggerElement.addEventListener("htmx:config-request", handler);
    },
  };

  var _pipeline = {
    configureRequest: function () {
      //add antiforgery cookie to the request headers
      document.addEventListener("htmx:config-request", function (evt) {
        // Get the antiforgery token from the RequestVerificationToken cookie
        // and add the request header so we're not dependent on hidden fields in forms
        const value = `; ${document.cookie}`;
        const parts = value.split("; RequestVerificationToken=");
        if (parts.length !== 2) {
          return;
        }
        evt.detail.headers["RequestVerificationToken"] = parts
          .pop()
          .split(";")
          .shift();
      });
    },

    handleErrorResponses: function () {
      document.addEventListener("htmx:response-error", function (evt) {
        var code = evt?.detail?.xhr?.status;
        var response = evt?.detail?.xhr?.responseText;
        if (response) {
          console.error(response);
        }
        window.location.assign(`/error?code=${code}`);
      });
    },
  };

  // initialization
  _theme.applyTheme();
  _theme.handleSystemThemeChange();

  _triggers.handleCloseModalTriggers();
  _triggers.handleFocusTriggers();
  _triggers.handleToastTriggers();

  _metadata.handleSetTriggers();
  _metadata.handleRemoveTriggers();

  _pipeline.configureRequest();
  _pipeline.handleErrorResponses();

  /**
   * razorx public API
   */
  return {
    /**
     * Puts the razorx client object into debug mode.
     * @param {Boolean} isDebugging True to output debug information to the console
     */
    debug: function (isDebugging) {
      _debug = isDebugging;
    },

    theme: {
      themeKey: _theme.themeKey,
      /**
       * Returns the name of the light theme.
       * @returns {String} The name of the light theme
       */
      getLightTheme: function () {
        return _theme.getLightTheme();
      },
      /**
       * Returns the name of the dark theme.
       * @returns {String} The name of the dark theme
       */
      getDarkTheme: function () {
        return _theme.getDarkTheme();
      },
      /**
       * Configures the light and dark theme names.
       * @param {String} light Sets the name of the light theme
       * @param {String} dark Sets the name of the dark theme
       */
      configureTheme: function (light, dark) {
        _theme.configureTheme(light, dark);
        _theme.applyTheme();
      },
      /**
       * Set the theme value on the document element based on the local storage value or media query.
       */
      applyTheme: function () {
        _theme.applyTheme();
      },
    },

    metadata: {
      scope: _metadata.scope,
      /**
       * Adds the metadata state object to the request.
       * GET and DELETE will send the state as a query parameter.
       * POST, PUT, and PATCH will send the state as a request header.
       * @param {Number} scope 0 (Transient), 1 (Session), or 2 (Persistent)
       * @param {String} key The unique key of the state object
       * @param {Element} triggerElement The element triggering the request
       */
      addToRequest: function (scope, key, triggerElement) {
        _metadata.addToRequest(scope, key, triggerElement);
      },
      /**
       * Persists the metadata state object on the client.
       * @param {Number} scope 0 (Transient), 1 (Session), or 2 (Persistent)
       * @param {String} key The unique key of the state object
       * @param {String} value The JSON stringified state object
       */
      set: function (scope, key, value) {
        _metadata.set(scope, key, value);
      },
    },
  };
})();
