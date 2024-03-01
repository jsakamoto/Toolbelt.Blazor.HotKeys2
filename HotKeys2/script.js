export var Toolbelt;
(function (Toolbelt) {
    var Blazor;
    (function (Blazor) {
        var HotKeys2;
        (function (HotKeys2) {
            class HotkeyEntry {
                constructor(dotNetObj, mode, modifiers, keyEntry, exclude, excludeSelector, isDisabled) {
                    this.dotNetObj = dotNetObj;
                    this.mode = mode;
                    this.modifiers = modifiers;
                    this.keyEntry = keyEntry;
                    this.exclude = exclude;
                    this.excludeSelector = excludeSelector;
                    this.isDisabled = isDisabled;
                }
                action() {
                    this.dotNetObj.invokeMethodAsync('InvokeAction');
                }
                updateDisabled(isDisabled) {
                    this.isDisabled = isDisabled;
                }
            }
            let idSeq = 0;
            const hotKeyEntries = new Map();
            HotKeys2.register = (dotNetObj, mode, modifiers, keyEntry, exclude, excludeSelector, isDisabled) => {
                const id = idSeq++;
                const hotKeyEntry = new HotkeyEntry(dotNetObj, mode, modifiers, keyEntry, exclude, excludeSelector, isDisabled);
                hotKeyEntries.set(id, hotKeyEntry);
                return id;
            };
            HotKeys2.unregister = (id) => {
                hotKeyEntries.delete(id);
            };
            HotKeys2.updateDisabled = (id, isDisabled) => {
                var _a;
                (_a = hotKeyEntries.get(id)) === null || _a === void 0 ? void 0 : _a.updateDisabled(isDisabled);
            };
            const convertToKeyNameMap = {
                "OS": "Meta",
                "Decimal": "Period",
            };
            const convertToKeyName = (ev) => {
                return convertToKeyNameMap[ev.key] || ev.key;
            };
            const OnKeyDownMethodName = "OnKeyDown";
            HotKeys2.attach = (hotKeysWrpper, isWasm) => {
                document.addEventListener('keydown', ev => {
                    if (typeof (ev["altKey"]) === 'undefined')
                        return;
                    const modifiers = (ev.shiftKey ? 1 : 0) +
                        (ev.ctrlKey ? 2 : 0) +
                        (ev.altKey ? 4 : 0) +
                        (ev.metaKey ? 8 : 0);
                    const key = convertToKeyName(ev);
                    const code = ev.code;
                    const targetElement = ev.target;
                    const tagName = targetElement.tagName;
                    const type = targetElement.getAttribute('type');
                    const preventDefault1 = onKeyDown(modifiers, key, code, targetElement, tagName, type);
                    const preventDefault2 = isWasm === true ? hotKeysWrpper.invokeMethod(OnKeyDownMethodName, modifiers, tagName, type, key, code) : false;
                    if (preventDefault1 || preventDefault2)
                        ev.preventDefault();
                    if (isWasm === false)
                        hotKeysWrpper.invokeMethodAsync(OnKeyDownMethodName, modifiers, tagName, type, key, code);
                });
            };
            const onKeyDown = (modifiers, key, code, targetElement, tagName, type) => {
                let preventDefault = false;
                hotKeyEntries.forEach(entry => {
                    if (!entry.isDisabled) {
                        const byCode = entry.mode === 1;
                        const eventKeyEntry = byCode ? code : key;
                        const keyEntry = entry.keyEntry;
                        if (keyEntry !== eventKeyEntry)
                            return;
                        const eventModkeys = byCode ? modifiers : (modifiers & (0xffff ^ 1));
                        let entryModKeys = byCode ? entry.modifiers : (entry.modifiers & (0xffff ^ 1));
                        if (keyEntry.startsWith("Shift") && byCode)
                            entryModKeys |= 1;
                        if (keyEntry.startsWith("Control"))
                            entryModKeys |= 2;
                        if (keyEntry.startsWith("Alt"))
                            entryModKeys |= 4;
                        if (keyEntry.startsWith("Meta"))
                            entryModKeys |= 8;
                        if (eventModkeys !== entryModKeys)
                            return;
                        if (isExcludeTarget(entry, targetElement, tagName, type))
                            return;
                        preventDefault = true;
                        entry.action();
                    }
                });
                return preventDefault;
            };
            const NonTextInputTypes = ["button", "checkbox", "color", "file", "image", "radio", "range", "reset", "submit",];
            const InputTageName = "INPUT";
            const isExcludeTarget = (entry, targetElement, tagName, type) => {
                if ((entry.exclude & 1) !== 0) {
                    if (tagName === InputTageName && NonTextInputTypes.every(t => t !== type))
                        return true;
                }
                if ((entry.exclude & 2) !== 0) {
                    if (tagName === InputTageName && NonTextInputTypes.some(t => t === type))
                        return true;
                }
                if ((entry.exclude & 4) !== 0) {
                    if (tagName === "TEXTAREA")
                        return true;
                }
                if ((entry.exclude & 8) !== 0) {
                    if (targetElement.isContentEditable)
                        return true;
                }
                if (entry.excludeSelector !== '' && targetElement.matches(entry.excludeSelector))
                    return true;
                return false;
            };
        })(HotKeys2 = Blazor.HotKeys2 || (Blazor.HotKeys2 = {}));
    })(Blazor = Toolbelt.Blazor || (Toolbelt.Blazor = {}));
})(Toolbelt || (Toolbelt = {}));
