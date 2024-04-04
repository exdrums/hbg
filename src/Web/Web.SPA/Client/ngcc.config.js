// DevExtreme: to remove "Entry point … contains deep imports" warnings.

module.exports = {
    packages: {
      'devextreme-angular':  {
        ignorableDeepImportMatchers: [
          /devextreme\//
        ]
      },
    }
  };