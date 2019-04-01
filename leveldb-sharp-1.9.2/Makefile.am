ACLOCAL_AMFLAGS = -I m4

OUTPUT_DIR = $(top_builddir)/bin/$(PROFILE)
OBJECT_DIR = $(OUTPUT_DIR)/obj
SOURCE_PATTERNS = *.cs
XBUILD_FLAGS += /p:Configuration=Debug /p:WarningLevel=0 /p:OutputPath=$(abspath $(OUTPUT_DIR)) /p:BaseIntermediateOutputPath=$(abspath $(OBJECT_DIR))/ /p:IntermediateOutputPath=$(abspath $(OBJECT_DIR))/

SOURCE_FILES = $(foreach pattern, $(SOURCE_PATTERNS), $(wildcard $(srcdir)/$(pattern)))
ASSEMBLY_TARGET = $(OUTPUT_DIR)/$(ASSEMBLY_NAME).dll
ASSEMBLY_PC = $(srcdir)/$(PACKAGE_NAME).pc
BUILD_FILE = $(srcdir)/leveldb-sharp.csproj
EXTRA_LIBS = $(ASSEMBLY_TARGET) $(ASSEMBLY_TARGET).mdb
GACUTIL_INSTALL = $(GACUTIL) -i $(ASSEMBLY_TARGET) -f $(GACUTIL_FLAGS)
GACUTIL_UNINSTALL = $(GACUTIL) -u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)

# magic automake variables
if INSTALL_PKG_LIB
pkgappdir = $(pkglibdir)
pkgapp_DATA = $(EXTRA_LIBS)
endif

if INSTALL_PKG_CONFIG
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = $(ASSEMBLY_PC)
endif

EXTRA_DIST = \
	$(SOURCE_FILES) $(BUILD_FILE) \
	$(srcdir)/README.md \
	$(srcdir)/LICENSE \
	$(srcdir)/leveldb-sharp.snk \
	$(srcdir)/leveldb-sharp.dll.config

CLEANFILES = \
	$(OUTPUT_DIR)/leveldb-sharp.dll.config
# end of magic

$(ASSEMBLY_TARGET) $(ASSEMBLY_TARGET).mdb: $(BUILD_FILE) $(SOURCE_FILES)
	$(XBUILD) $(XBUILD_FLAGS) $(BUILD_FILE)

clean-local:
	$(XBUILD) $(XBUILD_FLAGS) /t:Clean $(BUILD_FILE)

if INSTALL_PKG_GAC
install-data-local:
	echo "$(GACUTIL_INSTALL)";  \
	  $(GACUTIL_INSTALL)  || exit 1;

uninstall-local:
	echo "$(GACUTIL_UNINSTALL)";  \
	  $(GACUTIL_UNINSTALL)  || exit 1;
endif

# HACK: autofoo stubbornly creates the $pkglibdir/$package directory
if !INSTALL_PKG_LIB
installdirs:
install-pkglibDATA:
endif
