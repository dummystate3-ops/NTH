(function () {
    const STORAGE_KEY = 'productivityWorkspaceId';
    let workspacePromise = null;

    async function ensureWorkspace(name) {
        if (workspacePromise) {
            return workspacePromise;
        }

        workspacePromise = (async () => {
            const existingId = localStorage.getItem(STORAGE_KEY);
            const payload = existingId ? { workspaceId: existingId } : { name: name || 'Personal Workspace' };

            const response = await fetch('/api/productivity/workspaces/ensure', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                workspacePromise = null;
                throw new Error('Unable to initialize workspace');
            }

            const data = await response.json();
            localStorage.setItem(STORAGE_KEY, data.workspaceId);
            return data.workspaceId;
        })();

        return workspacePromise;
    }

    window.ProductivityWorkspace = {
        getId: async function () {
            return ensureWorkspace();
        },
        reset: function () {
            localStorage.removeItem(STORAGE_KEY);
            workspacePromise = null;
        }
    };
})();
