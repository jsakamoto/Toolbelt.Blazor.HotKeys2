var Toolbelt;
(function (Toolbelt) {
    var Blazor;
    (function (Blazor) {

        const numLockOverride = {
            state: undefined
        };

        // Override the getModifierState method to allow for NumLock to be set forcibly
        const getModifierState = KeyboardEvent.prototype.getModifierState;
        KeyboardEvent.prototype.getModifierState = function (modKeyName) {
            if (modKeyName === 'NumLock' && numLockOverride.state !== undefined) {
                return numLockOverride.state;
            }

            return getModifierState.apply(this, [modKeyName]);
        }

        Blazor.fireOnChange = (element) => {
            const event = new Event('change');
            element.dispatchEvent(event);
            console.log('onchange has been fired.', element)
        }

        Blazor.fireOnKeyDown = (args) => {

            if (typeof args.numLock !== 'undefined' && args.numLock !== null) {
                numLockOverride.state = args.numLock;
            }

            const element = document.querySelector(args.selector);
            const event = new KeyboardEvent("keydown", { ...args.options, ...{ bubbles: true } });
            element.dispatchEvent(event);

            numLockOverride.state = undefined;
        }

        Blazor.log = (text) => console.log(text);

    })(Blazor = Toolbelt.Blazor || (Toolbelt.Blazor = {}));
})(Toolbelt || (Toolbelt = {}));

