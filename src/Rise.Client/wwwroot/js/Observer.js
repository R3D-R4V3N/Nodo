window.Observer = {
    observer: null,
    Initialize: function (component, observerTargetId) {
        this.observer = new IntersectionObserver(_ => {
            component.invokeMethodAsync('OnIntersection');
        });

        let element = document.getElementById(observerTargetId);

        if (element == null)
            throw new Error("Target was not found");

        this.observer.observe(element);
    }
};