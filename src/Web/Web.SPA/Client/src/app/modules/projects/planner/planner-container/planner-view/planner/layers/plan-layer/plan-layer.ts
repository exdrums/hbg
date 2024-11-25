import * as L from "leaflet";

export interface PlanLayerOptions extends L.ImageOverlayOptions {
    editable?: boolean;
}

export class PlanLayer extends L.ImageOverlay {
    constructor(imageUrl: string, bounds: L.LatLngBoundsExpression, options: PlanLayerOptions) {
        super(imageUrl, bounds, options);
        if (options.editable)
            setTimeout(() => this.editing.enable(), 100);
    }

    private readonly editing: PlanLayerEditing = new PlanLayerEditing(this);

    public readonly changeEditing = (value: boolean) => value ? this.editing.enable() : this.editing.disable();
}

class PlanLayerEditing extends L.Edit.SimpleShape {
    private _overlay: L.ImageOverlay;
    private _rotationAngle: number = 0; // Rotation angle in radians
    private _moveMarker?: L.Marker;
    private _resizeMarkers: L.Marker[] = [];
    private _rotateMarker?: L.Marker;

    constructor(overlay: L.ImageOverlay) {
        super(overlay as any); // Explicit cast as L.Edit.SimpleShape expects specific types
        this._overlay = overlay;
    }

    // Enable editing (move, resize, rotate)
    enable() {
        this._createMoveMarker();
        this._createResizeMarkers();
        // TODO: implement working rotation flow
        // this._createRotateMarker();
        return this;
    }

    // Disable editing
    disable() {
        this._removeMarker(this._moveMarker);
        this._resizeMarkers.forEach((marker) => this._removeMarker(marker));
        this._resizeMarkers = [];
        this._removeMarker(this._rotateMarker);
        return this;
    }

    // Create move marker
    private _createMoveMarker(): void {
        const center = this._overlay.getBounds().getCenter();
        this._moveMarker = this._createMarker(center, 'leaflet-move-handle', this._onMove.bind(this));
    }

    // Create resize markers
    private _createResizeMarkers(): void {
        const bounds = this._overlay.getBounds();
        const corners = [
            bounds.getNorthWest(),
            bounds.getNorthEast(),
            bounds.getSouthWest(),
            bounds.getSouthEast(),
        ];

        this._resizeMarkers = corners.map((corner) =>
            this._createMarker(corner, 'leaflet-resize-handle', this._onResize.bind(this))
        );
    }

    // Create rotate marker
    private _createRotateMarker(): void {
        const rotatePosition = this._calculateRotateMarkerPosition();
        this._rotateMarker = this._createMarker(
            rotatePosition,
            'leaflet-rotate-handle',
            this._onRotate.bind(this)
        );
    }

    // Calculate the position of the rotate marker
    private _calculateRotateMarkerPosition(): L.LatLng {
        const bounds = this._overlay.getBounds();
        const topCenter = L.latLng(
            bounds.getNorthWest().lat,
            (bounds.getNorthWest().lng + bounds.getNorthEast().lng) / 2
        );

        // Offset above the top-center
        const offset = 0.0008; // Adjust as needed
        return L.latLng(topCenter.lat + offset, topCenter.lng);
    }

    // Create a marker
    private _createMarker(position: L.LatLng, className: string, dragHandler: (e: L.DragEndEvent) => void): L.Marker {
        const marker = new L.Marker(position, {
            draggable: true,
            icon: L.divIcon({ className: `leaflet-div-icon ${className}` }),
        });

        marker.on('drag', dragHandler, this);
        marker.addTo((this._overlay as any)._map!); // Use non-null assertion as the map will exist
        return marker;
    }

    // Remove a marker
    private _removeMarker(marker?: L.Marker): void {
        if (marker) {
            (this._overlay as any)._map!.removeLayer(marker);
        }
    }

    // Handle moving the image
    private _onMove(e: L.DragEndEvent): void {
        const center = e.target.getLatLng();
        const bounds = this._overlay.getBounds();
        const oldCenter = bounds.getCenter();

        // Calculate offset
        const latOffset = center.lat - oldCenter.lat;
        const lngOffset = center.lng - oldCenter.lng;

        // Update bounds
        const newBounds = L.latLngBounds(
            [bounds.getSouthWest().lat + latOffset, bounds.getSouthWest().lng + lngOffset],
            [bounds.getNorthEast().lat + latOffset, bounds.getNorthEast().lng + lngOffset]
        );

        this._overlay.setBounds(newBounds);
        this._updateMarkers();
    }

    // Handle resizing the image
    private _onResize(e: L.DragEndEvent): void {
        const marker = e.target;
        const bounds = this._overlay.getBounds();
        const center = bounds.getCenter();

        // Get the original corner positions
        const corners = [
            bounds.getNorthWest(),
            bounds.getNorthEast(),
            bounds.getSouthWest(),
            bounds.getSouthEast(),
        ];

        // Determine which corner is being dragged
        const index = this._resizeMarkers.indexOf(marker as L.Marker);
        if (index === -1) return;

        // The corner being dragged
        const draggedCorner = marker.getLatLng();

        // Determine the opposite corner to calculate proportional scaling
        const oppositeCorner = corners[(index + 2) % 4];

        // Calculate scaling factor based on the distance between corners
        const originalWidth = Math.abs(corners[1].lng - corners[0].lng);
        const originalHeight = Math.abs(corners[0].lat - corners[2].lat);
        const newWidth = Math.abs(draggedCorner.lng - oppositeCorner.lng);
        const newHeight = Math.abs(draggedCorner.lat - oppositeCorner.lat);

        // Choose the smaller scaling factor to maintain proportions
        const scaleFactor = Math.min(newWidth / originalWidth, newHeight / originalHeight);

        // Calculate new bounds based on the scale factor
        const newBounds = L.latLngBounds(
            L.latLng(
                center.lat - (originalHeight / 2) * scaleFactor,
                center.lng - (originalWidth / 2) * scaleFactor
            ),
            L.latLng(
                center.lat + (originalHeight / 2) * scaleFactor,
                center.lng + (originalWidth / 2) * scaleFactor
            )
        );

        // Update overlay bounds
        this._overlay.setBounds(newBounds);

        // Update markers
        this._updateMarkers();
    }

    // Handle rotating the image
    private _onRotate(e: L.DragEndEvent): void {
        const marker = e.target;
        const bounds = this._overlay.getBounds();
        const center = bounds.getCenter();

        // Calculate the angle between the center and the marker
        const dx = marker.getLatLng().lng - center.lng;
        const dy = marker.getLatLng().lat - center.lat;
        const angle = Math.atan2(dy, dx);

        // Calculate the delta angle for smooth rotation
        const deltaAngle = angle - this._rotationAngle;
        this._rotationAngle = angle;

        // Apply rotation to the image using CSS transform
        const overlayElement = this._overlay.getElement()!;
        overlayElement.style.transformOrigin = "center center"; // Set rotation origin to center
        overlayElement.style.transform = `rotate(${(this._rotationAngle * 180) / Math.PI}deg)`;

        // Update the position of the rotate marker
        const rotatePosition = this._calculateRotateMarkerPosition();
        (marker as L.Marker).setLatLng(rotatePosition);

        this._updateMarkers();
    }

    // Update all markers
    private _updateMarkers(): void {
        const bounds = this._overlay.getBounds();

        // Update move marker
        if (this._moveMarker) {
            this._moveMarker.setLatLng(bounds.getCenter());
        }

        // Update resize markers
        if (this._resizeMarkers) {
            const corners = [
                bounds.getNorthWest(),
                bounds.getNorthEast(),
                bounds.getSouthWest(),
                bounds.getSouthEast(),
            ];
            this._resizeMarkers.forEach((marker, index) =>
                marker.setLatLng(corners[index])
            );
        }

        // Update rotate marker
        if (this._rotateMarker) {
            const rotatePosition = this._calculateRotateMarkerPosition();
            this._rotateMarker.setLatLng(rotatePosition);
        }
    }
}