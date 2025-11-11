import DataGrid from 'devextreme/ui/data_grid';
import TabPanel from 'devextreme/ui/tab_panel';
import config from 'devextreme/core/config';
import DxSpeedDialAction from 'devextreme/ui/speed_dial_action';

export function applyDevExtremeDefaults() {
  DataGrid.defaultOptions({
    options: {
      width: "100%",
      height: "100%",
      columnAutoWidth: true,
      showBorders: true,
      rowAlternationEnabled: true,
      columnHidingEnabled: true,
      filterRow: {
        visible: true
      },
      headerFilter: {
        visible: true
      },
      searchPanel: {
        visible: true,
        width: 240,
        placeholder: "Search..."
      },
      paging: {
        pageSize: 20
      },
      pager: {
        visible: true,
        showPageSizeSelector: true,
        allowedPageSizes: [10, 20, 50, 100],
        showInfo: true
      }
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
