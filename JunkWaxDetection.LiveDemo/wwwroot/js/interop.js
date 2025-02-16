window.interop = {
    startWebcam: async function (videoElement) {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ video: { width: 1280, height: 720 } });
            videoElement.srcObject = stream;
            await videoElement.play();
        } catch (err) {
            console.error("Error starting webcam:", err);
        }
    },

    captureCroppedFrame: async function (videoElement, cropX, cropY, cropWidth, cropHeight) {
        const canvas = document.createElement("canvas");
        canvas.width = cropWidth;
        canvas.height = cropHeight;
        const ctx = canvas.getContext("2d");
        // Draw only the cropped area from the video.
        ctx.drawImage(videoElement, cropX, cropY, cropWidth, cropHeight, 0, 0, cropWidth, cropHeight);
        return canvas.toDataURL("image/jpeg", 0.75).replace(/^data:image\/(jpeg|jpg);base64,/, "");
    },

    // This function clears the overlay canvas, draws the cropping overlay (black bars),
    // and then draws the bounding boxes.
    updateOverlay: function (canvasElement, cropX, cropWidth, previewWidth, previewHeight, boxes, color, leftText, rightText) {
        const ctx = canvasElement.getContext("2d");
        // Clear the entire canvas.
        ctx.clearRect(0, 0, canvasElement.width, canvasElement.height);
        // Draw the cropping overlay (black bars on left and right).
        ctx.fillStyle = "black";
        // Left black bar: from x=0 to cropX.
        ctx.fillRect(0, 0, cropX, previewHeight);
        // Right black bar: from x = cropX+cropWidth to the end.
        ctx.fillRect(cropX + cropWidth, 0, previewWidth - (cropX + cropWidth), previewHeight);

        //Draw bounding boxes if they're specified
        if (boxes !== null) {
            ctx.strokeStyle = color;
            ctx.fillStyle = color;
            ctx.lineWidth = 3;
            ctx.font = "16px Arial";
            boxes.forEach(box => {
                ctx.beginPath();
                ctx.rect(box.x, box.y, box.width, box.height);
                ctx.stroke();
                ctx.fillText(`${box.label} (${box.confidence.toFixed(4)})`, box.x, box.y - 5);
            });
        }

        // Draw text in the blacked-out side areas.
        if (leftText) {
            this.drawSideText(canvasElement, "left", leftText, cropX, cropWidth, previewWidth, previewHeight);
        }

        if (rightText) {
            this.drawSideText(canvasElement, "right", rightText, cropX, cropWidth, previewWidth, previewHeight);
        }
    },

    /**
     * Draws multi-line text in the blacked-out side areas of the overlay canvas.
     * The function positions the text in the left or right area based on the 'side' parameter.
     * @param {HTMLCanvasElement} canvasElement - The overlay canvas element.
     * @param {string} side - Either "left" or "right" indicating which side to draw the text in.
     * @param {string} text - The text string to draw (can include newline characters).
     * @param {number} cropX - The x-coordinate where the crop region begins.
     * @param {number} cropWidth - The width of the crop region.
     * @param {number} previewWidth - The full width of the video preview.
     * @param {number} previewHeight - The full height of the video preview.
     */
    drawSideText: function (canvasElement, side, text, cropX, cropWidth, previewWidth, previewHeight) {
        const ctx = canvasElement.getContext("2d");
        // Set font and fill style for text.
        ctx.font = "16px Arial";
        ctx.fillStyle = "white";

        // Split the text into lines.
        const lines = text.split('\n');

        // Estimate the line height (adjust if needed)
        const lineHeight = 18; // pixels
        const totalTextHeight = lines.length * lineHeight;

        // Compute the starting vertical position to center the block of text.
        let startY = (previewHeight - totalTextHeight) / 2 + lineHeight;

        // For each line, measure its width and then compute x-coordinate for horizontal centering within the side area.
        lines.forEach((line, index) => {
            const textWidth = ctx.measureText(line).width;
            let x = 0;
            if (side === "left") {
                // The left area spans from x=0 to cropX.
                x = (cropX - textWidth) / 2;
            } else if (side === "right") {
                // The right area spans from cropX+cropWidth to previewWidth.
                x = cropX + cropWidth + ((previewWidth - (cropX + cropWidth)) - textWidth) / 2;
            }
            // Draw the line at the computed x and the y offset for this line.
            ctx.fillText(line, x, startY + index * lineHeight);
        });
    }

};
