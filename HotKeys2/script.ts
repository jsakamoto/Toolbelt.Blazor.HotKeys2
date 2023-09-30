export namespace Toolbelt.Blazor.HotKeys2 {

    const enum Exclude {
        None = 0,
        InputText = 0b0001,
        InputNonText = 0b0010,
        TextArea = 0b0100,
        ContentEditable = 0b1000
    }

    const enum ModCodes {
        None = 0,
        Shift = 0x01,
        Control = 0x02,
        Alt = 0x04,
        Meta = 0x08
    }

    const enum HotKeyMode {
        ByKey,
        ByCode
    }

    class HotkeyEntry {

        constructor(
            private dotNetObj: any,
            public mode: HotKeyMode,
            public modifiers: ModCodes,
            public keyEntry: string,
            public exclude: Exclude,
            public excludeSelector: string
        ) { }

        public action(): void {
            this.dotNetObj.invokeMethodAsync('InvokeAction');
        }
    }

    let idSeq: number = 0;
    const hotKeyEntries = new Map<number, HotkeyEntry>();

    export const register = (dotNetObj: any, mode: HotKeyMode, modifiers: ModCodes, keyEntry: string, exclude: Exclude, excludeSelector: string): number => {
        const id = idSeq++;
        const hotKeyEntry = new HotkeyEntry(dotNetObj, mode, modifiers, keyEntry, exclude, excludeSelector);
        hotKeyEntries.set(id, hotKeyEntry);
        return id;
    }

    export const unregister = (id: number): void => {
        hotKeyEntries.delete(id);
    }

    const convertToKeyNameMap: { [key: string]: string } = {
        "OS": "Meta",
        "Decimal": "Period",
    };

    const convertToKeyName = (ev: KeyboardEvent): string => {
        return convertToKeyNameMap[ev.key] || ev.key;
    }

    const OnKeyDownMethodName = "OnKeyDown";

    export const attach = (hotKeysWrpper: any, isWasm: boolean): void => {
        document.addEventListener('keydown', ev => {
            if (typeof (ev["altKey"]) === 'undefined') return;
            const modifiers =
                (ev.shiftKey ? ModCodes.Shift : 0) +
                (ev.ctrlKey ? ModCodes.Control : 0) +
                (ev.altKey ? ModCodes.Alt : 0) +
                (ev.metaKey ? ModCodes.Meta : 0);
            const key = convertToKeyName(ev);
            const code = ev.code;

            const srcElement = ev.srcElement as HTMLElement;
            const tagName = srcElement.tagName;
            const type = srcElement.getAttribute('type');

            const preventDefault1 = onKeyDown(modifiers, key, code, srcElement, tagName, type);
            const preventDefault2 = isWasm === true ? hotKeysWrpper.invokeMethod(OnKeyDownMethodName, modifiers, tagName, type, key, code) : false;
            if (preventDefault1 || preventDefault2) ev.preventDefault();
            if (isWasm === false) hotKeysWrpper.invokeMethodAsync(OnKeyDownMethodName, modifiers, tagName, type, key, code);
        });
    }

    const onKeyDown = (modifiers: ModCodes, key: string, code: string, srcElement: HTMLElement, tagName: string, type: string | null ): boolean => {
        let preventDefault = false;

        hotKeyEntries.forEach(entry => {

            const byCode = entry.mode === HotKeyMode.ByCode;
            const eventKeyEntry = byCode ? code : key;
            const keyEntry = entry.keyEntry;

            if (keyEntry !== eventKeyEntry) return;

            const eventModkeys = byCode ? modifiers : (modifiers & (0xffff ^ ModCodes.Shift));
            let entryModKeys = byCode ? entry.modifiers : (entry.modifiers & (0xffff ^ ModCodes.Shift));
            if (keyEntry.startsWith("Shift") && byCode) entryModKeys |= ModCodes.Shift;
            if (keyEntry.startsWith("Control")) entryModKeys |= ModCodes.Control;
            if (keyEntry.startsWith("Alt")) entryModKeys |= ModCodes.Alt;
            if (keyEntry.startsWith("Meta")) entryModKeys |= ModCodes.Meta;
            if (eventModkeys !== entryModKeys) return;

            if (!isAllowedIn(entry, srcElement, tagName, type)) return;

            if (entry.excludeSelector !== '' && srcElement.matches(entry.excludeSelector)) return;

            preventDefault = true;
            entry.action();
        });

        return preventDefault;
    }

    const NonTextInputTypes = ["button", "checkbox", "color", "file", "image", "radio", "range", "reset", "submit",];

    const InputTageName = "INPUT";

    const isAllowedIn = (entry: HotkeyEntry, srcElement: HTMLElement, tagName: string, type: string | null): boolean => {

        if ((entry.exclude & Exclude.InputText) !== 0) {
            if (tagName === InputTageName && NonTextInputTypes.indexOf(type || '') === -1) return false;
        }
        if ((entry.exclude & Exclude.InputNonText) !== 0) {
            if (tagName === InputTageName && NonTextInputTypes.indexOf(type || '') !== -1) return false;
        }
        if ((entry.exclude & Exclude.TextArea) !== 0) {
            if (tagName === "TEXTAREA") return false;
        }
        if ((entry.exclude & Exclude.ContentEditable) !== 0) {
            if (srcElement.contentEditable === "true") return false;
        }

        return true;
    }

}