const doc = document;
const OnKeyDownMethodName = "OnKeyDown";
const NonTextInputTypes = ["button", "checkbox", "color", "file", "image", "radio", "range", "reset", "submit",];
const InputTageName = "INPUT";
const keydown = "keydown";
class HotkeyEntry {
    constructor(dotNetObj, mode, modifiers, keyEntry, exclude, excludeSelector, state) {
        this.dotNetObj = dotNetObj;
        this.mode = mode;
        this.modifiers = modifiers;
        this.keyEntry = keyEntry;
        this.exclude = exclude;
        this.excludeSelector = excludeSelector;
        this.state = state;
    }
    action() {
        this.dotNetObj.invokeMethodAsync('InvokeAction');
    }
}
const addKeyDownEventListener = (listener) => doc.addEventListener(keydown, listener);
const removeKeyDownEventListener = (listener) => doc.removeEventListener(keydown, listener);
const convertToKeyName = (ev) => {
    const convertToKeyNameMap = {
        "OS": "Meta",
        "Decimal": "Period",
    };
    return convertToKeyNameMap[ev.key] || ev.key;
};
const startsWith = (str, prefix) => str.startsWith(prefix);
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
const createKeydownHandler = (callback) => {
    return (ev) => {
        if (typeof (ev["altKey"]) === 'undefined')
            return;
        const numLock = ev.getModifierState('NumLock');
        const shiftKey = (numLock && ev.code.match(/^Numpad([0-9]|Decimal)$/) && !ev.key.match(/^[0-9\.]$/)) ? true : ev.shiftKey;
        const modifiers = (shiftKey ? 1 : 0) +
            (ev.ctrlKey ? 2 : 0) +
            (ev.altKey ? 4 : 0) +
            (ev.metaKey ? 8 : 0);
        const key = convertToKeyName(ev);
        const code = ev.code;
        const targets = [ev.target, ev.composedPath()[0]]
            .filter(e => e)
            .map(e => [e, e.tagName, e.getAttribute('type')]);
        const preventDefault = callback(modifiers, key, code, targets);
        if (preventDefault)
            ev.preventDefault();
    };
};
export const createContext = () => {
    let idSeq = 0;
    const hotKeyEntries = new Map();
    const onKeyDown = (modifiers, key, code, targets) => {
        let preventDefault = false;
        hotKeyEntries.forEach(entry => {
            if (!entry.state.disabled) {
                const byCode = entry.mode === 1;
                const eventKeyEntry = byCode ? code : key;
                const keyEntry = entry.keyEntry;
                if (keyEntry !== eventKeyEntry)
                    return;
                const eventModkeys = byCode ? modifiers : (modifiers & (0xffff ^ 1));
                let entryModKeys = byCode ? entry.modifiers : (entry.modifiers & (0xffff ^ 1));
                if (startsWith(keyEntry, "Shift") && byCode)
                    entryModKeys |= 1;
                if (startsWith(keyEntry, "Control"))
                    entryModKeys |= 2;
                if (startsWith(keyEntry, "Alt"))
                    entryModKeys |= 4;
                if (startsWith(keyEntry, "Meta"))
                    entryModKeys |= 8;
                if (eventModkeys !== entryModKeys)
                    return;
                if (targets.some(([targetElement, tagName, type]) => isExcludeTarget(entry, targetElement, tagName, type)))
                    return;
                preventDefault = preventDefault || entry.state.preventDefault;
                entry.action();
            }
        });
        return preventDefault;
    };
    const keydownHandler = createKeydownHandler(onKeyDown);
    addKeyDownEventListener(keydownHandler);
    return {
        register: (dotNetObj, mode, modifiers, keyEntry, exclude, excludeSelector, initialState) => {
            const id = idSeq++;
            const hotKeyEntry = new HotkeyEntry(dotNetObj, mode, modifiers, keyEntry, exclude, excludeSelector, initialState);
            hotKeyEntries.set(id, hotKeyEntry);
            return id;
        },
        update: (id, state) => {
            const hotkeyEntry = hotKeyEntries.get(id);
            if (!hotkeyEntry)
                return;
            hotkeyEntry.state = state;
        },
        unregister: (id) => {
            if (id === -1)
                return;
            hotKeyEntries.delete(id);
        },
        dispose: () => { removeKeyDownEventListener(keydownHandler); }
    };
};
export const handleKeyEvent = (hotKeysWrapper, isWasm) => {
    const onKeyDown = (modifiers, key, code, targets) => {
        const [, tagName, type] = targets[0];
        if (isWasm) {
            return hotKeysWrapper.invokeMethod(OnKeyDownMethodName, modifiers, tagName, type, key, code);
        }
        else {
            hotKeysWrapper.invokeMethodAsync(OnKeyDownMethodName, modifiers, tagName, type, key, code);
            return false;
        }
    };
    const keydownHandler = createKeydownHandler(onKeyDown);
    addKeyDownEventListener(keydownHandler);
    return {
        dispose: () => { removeKeyDownEventListener(keydownHandler); }
    };
};
