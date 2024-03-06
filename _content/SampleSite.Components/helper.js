var Toolbelt;
(function (Toolbelt) {
    var Blazor;
    (function (Blazor) {
        Blazor.fireOnChange = (element) => {
            const event = new Event('change');
            element.dispatchEvent(event);
            console.log('onchange has been fired.', element)
        }

        Blazor.fireOnKeyDown = (args) => {
            const element = document.querySelector(args.selector);
            const event = new KeyboardEvent("keydown", { ...args.options, ...{ bubbles: true } });
            element.dispatchEvent(event);
        }

        Blazor.log = (text) => console.log(text);

    })(Blazor = Toolbelt.Blazor || (Toolbelt.Blazor = {}));
})(Toolbelt || (Toolbelt = {}));

