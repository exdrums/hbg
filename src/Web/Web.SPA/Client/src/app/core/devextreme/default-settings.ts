import DataGrid from 'devextreme/ui/data_grid';
import TabPanel from 'devextreme/ui/tab_panel';
import config from 'devextreme/core/config';
import DxSpeedDialAction from 'devextreme/ui/speed_dial_action';

export function applyDevExtremeDefaults() {
    DataGrid.defaultOptions({
        options: {
            width: "100%",
            height: "100%",
            columnAutoWidth: true
        }
    });
    TabPanel.defaultOptions({
        options: {
            animationEnabled: true
        }
    });
    DxSpeedDialAction.defaultOptions({
        options: {
            icon: 'rowfield',
            shading: true,
            direction: 'down',
            position: {
                my: 'right bottom',
                at: 'right bottom',
                offset: '-35 -16',
            }
        }
    });
}