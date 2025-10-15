(function () {
    let modelPromise;

    async function loadModel() {
        if (!modelPromise) {
            if (!window.nsfwjs) {
                throw new Error("nsfwjs library is niet geladen.");
            }
            modelPromise = window.nsfwjs.load();
        }
        return modelPromise;
    }

    async function validateImage(dataUrl) {
        try {
            const model = await loadModel();
            const img = await createImage(dataUrl);
            const predictions = await model.classify(img);

            const blockedCategories = ["Porn", "Hentai", "Sexy"];
            let highestBlockedProbability = 0;

            for (const prediction of predictions) {
                if (blockedCategories.includes(prediction.className)) {
                    highestBlockedProbability = Math.max(highestBlockedProbability, prediction.probability);
                }
            }

            // Keur goed als geen van de ongewenste categorieën meer dan 0.4 scoort
            return highestBlockedProbability < 0.4;
        } catch (error) {
            console.error("Kon de afbeelding niet valideren", error);
            return false;
        }
    }

    function showResult(isApproved) {
        if (isApproved) {
            alert("Profielfoto goedgekeurd! Deze afbeelding is geschikt.");
        } else {
            alert("Profielfoto afgekeurd. Kies een foto zonder naaktheid of badkleding.");
        }
    }

    function createImage(dataUrl) {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => resolve(img);
            img.onerror = (err) => reject(err);
            img.src = dataUrl;
        });
    }

    window.profilePictureValidation = {
        validateImage,
        showResult
    };
})();
