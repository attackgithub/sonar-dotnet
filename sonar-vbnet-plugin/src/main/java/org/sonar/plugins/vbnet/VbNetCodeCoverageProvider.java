/*
 * SonarVB
 * Copyright (C) 2012-2018 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonar.plugins.vbnet;

import java.util.Arrays;
import java.util.List;
import org.sonar.api.config.Configuration;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.resources.Qualifiers;
import org.sonar.plugins.dotnet.tests.CoverageAggregator;
import org.sonar.plugins.dotnet.tests.CoverageConfiguration;
import org.sonar.plugins.dotnet.tests.CoverageReportImportSensor;

public class VbNetCodeCoverageProvider {

  private static final String CATEGORY = "VB.NET";
  private static final String SUBCATEGORY = "Code Coverage";

  private static final String NCOVER3_PROPERTY_KEY = "sonar.vbnet.ncover3.reportsPaths";
  private static final String OPENCOVER_PROPERTY_KEY = "sonar.vbnet.opencover.reportsPaths";
  private static final String DOTCOVER_PROPERTY_KEY = "sonar.vbnet.dotcover.reportsPaths";
  private static final String VISUAL_STUDIO_COVERAGE_XML_PROPERTY_KEY = "sonar.vbnet.vscoveragexml.reportsPaths";

  private static final String IT_NCOVER3_PROPERTY_KEY = "sonar.vbnet.ncover3.it.reportsPaths";
  private static final String IT_OPENCOVER_PROPERTY_KEY = "sonar.vbnet.opencover.it.reportsPaths";
  private static final String IT_DOTCOVER_PROPERTY_KEY = "sonar.vbnet.dotcover.it.reportsPaths";
  private static final String IT_VISUAL_STUDIO_COVERAGE_XML_PROPERTY_KEY = "sonar.vbnet.vscoveragexml.it.reportsPaths";

  private static final CoverageConfiguration COVERAGE_CONF = new CoverageConfiguration(
    VbNetPlugin.LANGUAGE_KEY,
    NCOVER3_PROPERTY_KEY,
    OPENCOVER_PROPERTY_KEY,
    DOTCOVER_PROPERTY_KEY,
    VISUAL_STUDIO_COVERAGE_XML_PROPERTY_KEY);

  private static final CoverageConfiguration IT_COVERAGE_CONF = new CoverageConfiguration(
    VbNetPlugin.LANGUAGE_KEY,
    IT_NCOVER3_PROPERTY_KEY,
    IT_OPENCOVER_PROPERTY_KEY,
    IT_DOTCOVER_PROPERTY_KEY,
    IT_VISUAL_STUDIO_COVERAGE_XML_PROPERTY_KEY);

  private VbNetCodeCoverageProvider() {
  }

  public static List extensions() {
    return Arrays.asList(
      VbNetCoverageAggregator.class, VbNetIntegrationCoverageAggregator.class,
      VbNetCoverageReportImportSensor.class, VbNetIntegrationCoverageReportImportSensor.class,

      PropertyDefinition.builder(NCOVER3_PROPERTY_KEY)
        .name("NCover3 Unit Tests Reports Paths")
        .description("Example: \"report.nccov\", \"report1.nccov,report2.nccov\" or \"C:/report.nccov\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(IT_NCOVER3_PROPERTY_KEY)
        .name("NCover3 Integration Tests Reports Paths")
        .description("Example: \"report.nccov\", \"report1.nccov,report2.nccov\" or \"C:/report.nccov\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(OPENCOVER_PROPERTY_KEY)
        .name("OpenCover Unit Tests Reports Paths")
        .description("Example: \"report.xml\", \"report1.xml,report2.xml\" or \"C:/report.xml\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(IT_OPENCOVER_PROPERTY_KEY)
        .name("OpenCover Integration Tests Reports Paths")
        .description("Example: \"report.xml\", \"report1.xml,report2.xml\" or \"C:/report.xml\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(DOTCOVER_PROPERTY_KEY)
        .name("dotCover Unit Tests (HTML) Reports Paths")
        .description("Example: \"report.html\", \"report1.html,report2.html\" or \"C:/report.html\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(IT_DOTCOVER_PROPERTY_KEY)
        .name("dotCover Integration Tests (HTML) Reports Paths")
        .description("Example: \"report.html\", \"report1.html,report2.html\" or \"C:/report.html\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(VISUAL_STUDIO_COVERAGE_XML_PROPERTY_KEY)
        .name("Visual Studio Unit Tests (XML) Reports Paths")
        .description("Example: \"report.coveragexml\", \"report1.coveragexml,report2.coveragexml\" or \"C:/report.coveragexml\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build(),
      PropertyDefinition.builder(IT_VISUAL_STUDIO_COVERAGE_XML_PROPERTY_KEY)
        .name("Visual Studio Integration Tests (XML) Reports Paths")
        .description("Example: \"report.coveragexml\", \"report1.coveragexml,report2.coveragexml\" or \"C:/report.coveragexml\"")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onlyOnQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .multiValues(true)
        .build());
  }

  public static class VbNetCoverageAggregator extends CoverageAggregator {

    public VbNetCoverageAggregator(Configuration configuration) {
      super(COVERAGE_CONF, configuration);
    }

  }

  public static class VbNetCoverageReportImportSensor extends CoverageReportImportSensor {

    public VbNetCoverageReportImportSensor(VbNetCoverageAggregator coverageAggregator) {
      super(COVERAGE_CONF, coverageAggregator, VbNetPlugin.LANGUAGE_KEY, VbNetPlugin.LANGUAGE_NAME, false);
    }

  }

  public static class VbNetIntegrationCoverageAggregator extends CoverageAggregator {

    public VbNetIntegrationCoverageAggregator(Configuration configuration) {
      super(IT_COVERAGE_CONF, configuration);
    }

  }

  public static class VbNetIntegrationCoverageReportImportSensor extends CoverageReportImportSensor {

    public VbNetIntegrationCoverageReportImportSensor(VbNetIntegrationCoverageAggregator coverageAggregator) {
      super(IT_COVERAGE_CONF, coverageAggregator, VbNetPlugin.LANGUAGE_KEY, VbNetPlugin.LANGUAGE_NAME, true);
    }

  }

}